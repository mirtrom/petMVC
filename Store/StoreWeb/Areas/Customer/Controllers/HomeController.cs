using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StoreDataAccess.Constants;
using StoreDataAccess.Interfaces;
using StoreDataAccess.Models;
using StoreDataAccess.Models.Data;
using System.Diagnostics;
using System.Security.Claims;

namespace StoreWeb.Areas.Customer.Controllers
{
	[Area("Customer")]
	public class HomeController : Controller
	{
		private readonly IUnitOfWork unitOfWork;

		public HomeController(StoreDbContext context)
		{
			unitOfWork = new UnitOfWork(context);
		}

		public async Task<IActionResult> Index()
		{
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
			if (claim != null)
			{
                var cards = await unitOfWork.ShoppingCardRepository.GetAllAsync();
                var count = cards.Where(sc => sc.StoreUserId == claim.Value).Count();
                HttpContext.Session.SetInt32(NamingConstants.CartSession, count);

            }
            IEnumerable<Product> Products = await unitOfWork.ProductRepository.GetAllAsync();
			return View(Products);
		}

		public async Task<IActionResult> Details(int productId)
		{
			var product = await unitOfWork.ProductRepository.GetByIdWithDetailsAsync(productId);
			var shoppingCard = new ShoppingCard()
			{
                Product = product,
                Quantity = 1,
                ProductId = productId,
            };
			return View(shoppingCard);
		}

		[HttpPost]
		[Authorize]
        public async Task<IActionResult> Details(ShoppingCard shoppingCard)
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
            var storeUserId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
			shoppingCard.StoreUserId = storeUserId;
			var shoppingCardsFromDb = await unitOfWork.ShoppingCardRepository.GetAllAsync();
			var shoppingCardFromDb = shoppingCardsFromDb.FirstOrDefault(sc => sc.ProductId == shoppingCard.ProductId && sc.StoreUserId == storeUserId);
			if (shoppingCardFromDb != null)
			{
                shoppingCardFromDb.Quantity += shoppingCard.Quantity;
				unitOfWork.ShoppingCardRepository.Update(shoppingCardFromDb);
                await unitOfWork.SaveAsync();
            }
			else
			{
                await unitOfWork.ShoppingCardRepository.AddAsync(shoppingCard);
                await unitOfWork.SaveAsync();
				var cards = await unitOfWork.ShoppingCardRepository.GetAllAsync();
				var count = cards.Where(sc => sc.StoreUserId == storeUserId).Count();
                HttpContext.Session.SetInt32(NamingConstants.CartSession, count);
            }
			TempData["success"] = "Product added to shopping card successfully";
            return RedirectToAction("Index");
        }


        public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}