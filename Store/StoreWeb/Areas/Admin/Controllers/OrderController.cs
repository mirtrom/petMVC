using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreDataAccess.Constants;
using StoreDataAccess.Interfaces;
using StoreDataAccess.Models;
using StoreDataAccess.Models.Data;
using StoreDataAccess.ViewModels;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace StoreWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        [BindProperty]
        public OrderVM OrderVM { get; set; }
        public OrderController(StoreDbContext context)
        {
            unitOfWork = new UnitOfWork(context);
        }
        public IActionResult Index()
        {
            return View();
        }
		public async Task<IActionResult> Details(int orderId)
		{
            OrderVM = new OrderVM()
            {
                OrderHeader = await unitOfWork.OrderHeaderRepository.GetByIdWithDetailsAsync(orderId),
				OrderDetails = await unitOfWork.OrderDetailRepository.GetAllByOrderHeaderAsync(orderId),
			};
			return View(OrderVM);
		}

        [Authorize(Roles = RoleConstants.Role_Admin + "," + RoleConstants.Role_Employee)]
        public async Task<IActionResult> UpdateOrderDetail()
        {
            var order = await unitOfWork.OrderHeaderRepository.GetByIdAsync(OrderVM.OrderHeader.Id);
            order.Name = OrderVM.OrderHeader.Name;
            order.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            order.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            order.City = OrderVM.OrderHeader.City;
            order.Region = OrderVM.OrderHeader.Region;
            order.PostalCode = OrderVM.OrderHeader.PostalCode;
            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.Carrier))
            {
                order.Carrier = OrderVM.OrderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.TrackingNumber))
            {
                order.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            }
            unitOfWork.OrderHeaderRepository.Update(order);
            await unitOfWork.SaveAsync();

            TempData["Success"] = "Order Details Updated Successfully.";


            return RedirectToAction(nameof(Details), new { orderId = order.Id });
        }

        [HttpPost]
        [Authorize(Roles = RoleConstants.Role_Admin + "," + RoleConstants.Role_Employee)]
        public async Task<IActionResult> StartProcessing()
        {
            unitOfWork.OrderHeaderRepository.UpdateStatus(OrderVM.OrderHeader.Id, OrderStatusConstants.Status_InProcess);
            await unitOfWork.SaveAsync();
            TempData["Success"] = "Order is now in process.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = RoleConstants.Role_Admin + "," + RoleConstants.Role_Employee)]
        public async Task<IActionResult> ShipOrder()
        {
            var order = await unitOfWork.OrderHeaderRepository.GetByIdAsync(OrderVM.OrderHeader.Id);
            order.Carrier = OrderVM.OrderHeader.Carrier;
            order.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            order.ShippingDate = DateTime.Now;
            order.OrderStatus = OrderStatusConstants.Status_Shipped;
            if (order.PaymentStatus == PaymentConstants.PaymentStatus_DelayedPayment)
            {
                order.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }
            unitOfWork.OrderHeaderRepository.UpdateStatus(OrderVM.OrderHeader.Id, OrderStatusConstants.Status_Shipped);
            await unitOfWork.SaveAsync();
            TempData["Success"] = "Order is now shipped.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = RoleConstants.Role_Admin + "," + RoleConstants.Role_Employee)]
        public async Task<IActionResult> CancelOrder()
        {
            var order = await unitOfWork.OrderHeaderRepository.GetByIdAsync(OrderVM.OrderHeader.Id);
            if (order.PaymentStatus == PaymentConstants.PaymentStatus_Approved)
            {
                var options = new RefundCreateOptions
                {
                    Amount = Convert.ToInt32(order.OrderTotal * 100),
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = order.PaymentIntendId,
                };

                var service = new RefundService();
                Refund refund = service.Create(options);

                unitOfWork.OrderHeaderRepository.UpdateStatus(OrderVM.OrderHeader.Id, OrderStatusConstants.Status_Cancelled, PaymentConstants.PaymentStatus_Rejected);
            }
            else
            {
                unitOfWork.OrderHeaderRepository.UpdateStatus(OrderVM.OrderHeader.Id, OrderStatusConstants.Status_Cancelled);
            }
            await unitOfWork.SaveAsync();
            TempData["Success"] = "Order is cancelled successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ActionName("Details")]
        public async Task<IActionResult> DetailsPayNow()
        {
            OrderVM.OrderHeader = await unitOfWork.OrderHeaderRepository.GetByIdWithDetailsAsync(OrderVM.OrderHeader.Id);
            OrderVM.OrderDetails = await unitOfWork.OrderDetailRepository.GetAllByOrderHeaderAsync(OrderVM.OrderHeader.Id);

            var domain = "https://localhost:7130/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"Admin/Order/PaymentConfirmation?id={OrderVM.OrderHeader.Id}",
                CancelUrl = domain + $"Admin/order/details?orderId={OrderVM.OrderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in OrderVM.OrderDetails)
            {
                var sessionLineItemOptions = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)item.Product.Price * 100,
                        Currency = "uah",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name
                        },
                    },
                    Quantity = item.Quantity,
                };
                options.LineItems.Add(sessionLineItemOptions);
            }

            var service = new SessionService();
            Session session = service.Create(options);
            unitOfWork.OrderHeaderRepository.UpdateStripePaymentId(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            await unitOfWork.SaveAsync();
            Response.Headers.Add("Location", session.Url);
            return StatusCode(303);
        }

        public async Task<IActionResult> PaymentConfirmation(int id)
        {
            var orderHeader = await unitOfWork.OrderHeaderRepository.GetByIdWithDetailsAsync(id);
            if (orderHeader.PaymentStatus == PaymentConstants.PaymentStatus_DelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if (session.PaymentStatus == "paid")
                {
                    unitOfWork.OrderHeaderRepository.UpdateStripePaymentId(id, session.Id, session.PaymentIntentId);
                    unitOfWork.OrderHeaderRepository.UpdateStatus(id, orderHeader.OrderStatus, PaymentConstants.PaymentStatus_Approved);
                    await unitOfWork.SaveAsync();
                }
            }

            return View(id);
        }

        #region API CALLS

        [HttpGet]
        public async Task<IActionResult> GetAll(string status)
        {
            IEnumerable<OrderHeader> orderHeaders;
            if(User.IsInRole(RoleConstants.Role_Admin) || User.IsInRole(RoleConstants.Role_Employee))
            {
                orderHeaders = await unitOfWork.OrderHeaderRepository.GetAllWithDetailsAsync();
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                orderHeaders = await unitOfWork.OrderHeaderRepository.GetByUserIdAsync(userId);
            }
            if (status == "pending")
            {
				orderHeaders = orderHeaders.Where(o => o.PaymentStatus == PaymentConstants.PaymentStatus_DelayedPayment);
			}
			else if (status == "inprocess")
            {
                orderHeaders = orderHeaders.Where(o => o.OrderStatus == OrderStatusConstants.Status_InProcess);
			}
			else if (status == "completed")
            {
				orderHeaders = orderHeaders.Where(o => o.OrderStatus == OrderStatusConstants.Status_Shipped);
			}
			else if (status == "approved")
            {
				orderHeaders = orderHeaders.Where(o => o.OrderStatus == OrderStatusConstants.Status_Approved);
			}   

            return Json(new { data = orderHeaders });
        }

        #endregion
    }
}
