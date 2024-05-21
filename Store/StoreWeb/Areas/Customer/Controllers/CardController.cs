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

namespace StoreWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CardController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        [BindProperty]
		public ShoppingCardVM ShoppingCardVM { get; set; }

		public CardController(StoreDbContext context)
        {
            unitOfWork = new UnitOfWork(context);
        }
        public async Task<IActionResult> Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var storeUserId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            var shoppingCardList = await unitOfWork.ShoppingCardRepository.GetShoppingCardListAsync(storeUserId);
            var shoppingCardVM = new ShoppingCardVM()
            {
                ShoppingCardList = shoppingCardList,
                OrderHeader = new OrderHeader()
            };
            shoppingCardVM.OrderHeader.OrderTotal += shoppingCardList.Sum(s => (double)s.Product.Price * s.Quantity);
            return View(shoppingCardVM);
        }
        public async Task<IActionResult> Plus(int cardId)
        {
            var shoppingCard = await unitOfWork.ShoppingCardRepository.GetByIdAsync(cardId);
            shoppingCard.Quantity += 1;
            unitOfWork.ShoppingCardRepository.Update(shoppingCard);
            await unitOfWork.SaveAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Minus(int cardId)
        {
            var shoppingCard = await unitOfWork.ShoppingCardRepository.GetByIdAsync(cardId);
            if (shoppingCard.Quantity <= 1)
            {
                var cards = await unitOfWork.ShoppingCardRepository.GetAllAsync();
                var count = cards.Where(sc => sc.StoreUserId == shoppingCard.StoreUserId).Count() - 1;
                HttpContext.Session.SetInt32(NamingConstants.CartSession, count);
                unitOfWork.ShoppingCardRepository.Delete(shoppingCard);

            }
            else
            {
                shoppingCard.Quantity -= 1;
                unitOfWork.ShoppingCardRepository.Update(shoppingCard);
            }
            await unitOfWork.SaveAsync();
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Remove(int cardId)
        {
            var shoppingCard = await unitOfWork.ShoppingCardRepository.GetByIdAsync(cardId);
            var cards = await unitOfWork.ShoppingCardRepository.GetAllAsync();
            var count = cards.Where(sc => sc.StoreUserId == shoppingCard.StoreUserId).Count() - 1;
            HttpContext.Session.SetInt32(NamingConstants.CartSession, count);
            unitOfWork.ShoppingCardRepository.Delete(shoppingCard);
            await unitOfWork.SaveAsync();
            return RedirectToAction("Index");
        }

		public ShoppingCardVM GetShoppingCardVM()
		{
			return ShoppingCardVM;
		}

		public async Task<IActionResult> Checkout()
        {
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var storeUserId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
			var shoppingCardList = await unitOfWork.ShoppingCardRepository.GetShoppingCardListAsync(storeUserId);
			ShoppingCardVM = new ShoppingCardVM()
			{
				ShoppingCardList = shoppingCardList,
				OrderHeader = new OrderHeader(),
			};
			ShoppingCardVM.OrderHeader.StoreUser = await unitOfWork.StoreUserRepository.GetByIdAsync(storeUserId);
			ShoppingCardVM.OrderHeader.Name = ShoppingCardVM.OrderHeader.StoreUser.Name;
			ShoppingCardVM.OrderHeader.PhoneNumber = ShoppingCardVM.OrderHeader.StoreUser.PhoneNumber;
			ShoppingCardVM.OrderHeader.PostalCode = ShoppingCardVM.OrderHeader.StoreUser.PostalCode;
			ShoppingCardVM.OrderHeader.Region = ShoppingCardVM.OrderHeader.StoreUser.Region;
			ShoppingCardVM.OrderHeader.City = ShoppingCardVM.OrderHeader.StoreUser.City;
			ShoppingCardVM.OrderHeader.StreetAddress = ShoppingCardVM.OrderHeader.StoreUser.StreetAddress;

			ShoppingCardVM.OrderHeader.OrderTotal = shoppingCardList.Sum(s => (double)s.Product.Price * s.Quantity);

			return View(ShoppingCardVM);
		}

        [HttpPost]
        [ActionName("Checkout")]
		public async Task<IActionResult> CheckoutPOST(ShoppingCardVM shoppingCardVM)
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var storeUserId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
			var shoppingCardList = await unitOfWork.ShoppingCardRepository.GetShoppingCardListAsync(storeUserId);
			ShoppingCardVM.ShoppingCardList = shoppingCardList;
			ShoppingCardVM.OrderHeader.OrderDate = DateTime.Now;
			ShoppingCardVM.OrderHeader.StoreUserId = storeUserId;
			ShoppingCardVM.OrderHeader.StoreUser = await unitOfWork.StoreUserRepository.GetByIdAsync(storeUserId);
			ShoppingCardVM.OrderHeader.OrderTotal = shoppingCardList.Sum(s => (double)s.Product.Price * s.Quantity);

            if (ShoppingCardVM.OrderHeader.StoreUser.CompanyId.GetValueOrDefault() == 0)
            {
                ShoppingCardVM.OrderHeader.PaymentStatus = PaymentConstants.PaymentStatus_Pending;
                ShoppingCardVM.OrderHeader.OrderStatus = OrderStatusConstants.Status_Pending;
            }
            else
            {
                ShoppingCardVM.OrderHeader.PaymentStatus = PaymentConstants.PaymentStatus_DelayedPayment;
                ShoppingCardVM.OrderHeader.OrderStatus = OrderStatusConstants.Status_Approved;
			}
            await unitOfWork.OrderHeaderRepository.AddAsync(ShoppingCardVM.OrderHeader);
            await unitOfWork.SaveAsync();
            foreach (var item in shoppingCardList)
            {
				OrderDetail orderDetail = new OrderDetail()
                {
					ProductId = item.ProductId,
                    OrderHeaderId = ShoppingCardVM.OrderHeader.Id,
					Price = (double)item.Product.Price,
					Quantity = item.Quantity
				};
                
				await unitOfWork.OrderDetailRepository.AddAsync(orderDetail);
                await unitOfWork.SaveAsync();
            }

			await unitOfWork.SaveAsync();

			if (ShoppingCardVM.OrderHeader.StoreUser.CompanyId.GetValueOrDefault() == 0)
			{
                var domain = "https://localhost:7130/";
                var options = new SessionCreateOptions
                {
                    SuccessUrl = domain + $"Customer/Card/OrderConfirmation?id={ShoppingCardVM.OrderHeader.Id}",
                    CancelUrl = domain + "Customer/Card/Index",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };

                foreach (var item in shoppingCardList)
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
                unitOfWork.OrderHeaderRepository.UpdateStripePaymentId(ShoppingCardVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                await unitOfWork.SaveAsync();
                Response.Headers.Add("Location", session.Url);
                return StatusCode(303);

            }
            return RedirectToAction("OrderConfirmation", new { id = ShoppingCardVM.OrderHeader.Id });
		}

        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var orderHeader = await unitOfWork.OrderHeaderRepository.GetByIdWithDetailsAsync(id);
            if(orderHeader.PaymentStatus != PaymentConstants.PaymentStatus_DelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if(session.PaymentStatus == "paid")
                {
                    unitOfWork.OrderHeaderRepository.UpdateStripePaymentId(orderHeader.Id, session.Id, session.PaymentIntentId);
                    unitOfWork.OrderHeaderRepository.UpdateStatus(orderHeader.Id, OrderStatusConstants.Status_Approved, PaymentConstants.PaymentStatus_Approved);
                    await unitOfWork.SaveAsync();
                }
            }
            IEnumerable<ShoppingCard> shoppingCardList = await unitOfWork.ShoppingCardRepository.GetShoppingCardListAsync(orderHeader.StoreUserId);
            foreach (var item in shoppingCardList)
            {
                unitOfWork.ShoppingCardRepository.Delete(item);
            }
            await unitOfWork.SaveAsync();
            return View(id);
		}
	}
}
