using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StoreDataAccess.Constants;
using StoreDataAccess.Interfaces;
using StoreDataAccess.Models;
using StoreDataAccess.Models.Data;
using StoreDataAccess.ViewModels;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StoreWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RoleConstants.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

		public IEnumerable<Product> Products;
		private readonly IWebHostEnvironment webHostEnvironment;
        public ProductController(StoreDbContext context, IWebHostEnvironment webHostEnv)
        {
            unitOfWork = new UnitOfWork(context);
			webHostEnvironment = webHostEnv;
        }
        public async Task<IActionResult> Index()
        {
            Products = await unitOfWork.ProductRepository.GetAllWithDetailsAsync();
            return View(Products);
        }
		public async Task<IActionResult> Create()
		{
			var categoryList = await unitOfWork.CategoryRepository.GetAllAsync();
			var categories = categoryList.Select(c =>
				new SelectListItem
				{
					Text = c.Name,
					Value = c.Id.ToString()
				}).ToList(); // Make sure to convert to a list
			return View(new ProductVM { Product = new Product(), CategoryList = categories });
		}

		[HttpPost]
		public async Task<IActionResult> Create(ProductVM productVM, IFormFile file)
		{
			if (ModelState.IsValid)
			{
				string wwwRootPath = webHostEnvironment.WebRootPath;
				if (file != null)
				{
					string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
					string productPath = Path.Combine(wwwRootPath, @"images\product");
					using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
					{
						file.CopyTo(fileStream);
					}
					productVM.Product.ImageUrl = @"images\product\" + fileName;
				}
				await unitOfWork.ProductRepository.AddAsync(productVM.Product);
				await unitOfWork.SaveAsync();
				TempData["Success"] = "Product added successfully";
				return RedirectToAction("Index");
			}
			else
			{
				TempData["Error"] = "Product not added";

				// Repopulate the CategoryList when ModelState is invalid
				var categoryList = await unitOfWork.CategoryRepository.GetAllAsync();
				productVM.CategoryList = categoryList.Select(c =>
					new SelectListItem
					{
						Text = c.Name,
						Value = c.Id.ToString()
					}).ToList();
			}

			// Always return the view with the product view model
			return View(productVM);
		}


		public async Task<IActionResult> Edit(int id)
		{
			var categoryList = await unitOfWork.CategoryRepository.GetAllAsync();
			var categories = categoryList.Select(c =>
				new SelectListItem
				{
					Text = c.Name,
					Value = c.Id.ToString()
				}).ToList();

			if (id == 0)
			{
				return NotFound();
			}

			var product = await unitOfWork.ProductRepository.GetByIdAsync(id);
			if (product == null)
			{
				return NotFound();
			}

			var productVM = new ProductVM()
			{
				Product = product,
				CategoryList = categories
			};

			return View(productVM);
		}

		[HttpPost]
		public async Task<IActionResult> Edit(ProductVM productVM, IFormFile file)
		{
			if (ModelState.IsValid)
			{
				string wwwRootPath = webHostEnvironment.WebRootPath;
				if (file != null)
				{
					// Deleting the old image if it exists
					if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
					{
						var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));
						if (System.IO.File.Exists(oldImagePath))
						{
							System.IO.File.Delete(oldImagePath);
						}
					}

					// Uploading the new image
					string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
					string productPath = Path.Combine(wwwRootPath, @"images\product");
					using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
					{
						file.CopyTo(fileStream);
					}
					productVM.Product.ImageUrl = @"images\product\" + fileName;
				}

				unitOfWork.ProductRepository.Update(productVM.Product);
				await unitOfWork.SaveAsync();
				TempData["Success"] = "Product updated successfully";
				return RedirectToAction("Index", "Product");
			}
			else
			{
				// Repopulate the CategoryList if model validation fails
				var categoryList = await unitOfWork.CategoryRepository.GetAllAsync();
				productVM.CategoryList = categoryList.Select(c =>
					new SelectListItem
					{
						Text = c.Name,
						Value = c.Id.ToString()
					}).ToList();

				TempData["Error"] = "Product update failed";
				return View(productVM);
			}
		}
		public async Task<IActionResult> Delete(int id)
		{
			if (id == 0)
			{
				return NotFound();
			}
			var product = await unitOfWork.ProductRepository.GetByIdAsync(id);
			if (product == null)
			{
				return NotFound();
			}
			return View(product);
		}
		[HttpPost, ActionName("Delete")]
		public async Task<IActionResult> Delete(Product product)
		{
			unitOfWork.ProductRepository.Delete(product);
			await unitOfWork.SaveAsync();
			TempData["Success"] = "Product deleted successfully";
			return RedirectToAction("Index", "Product");
		}


        #region API CALLS

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await unitOfWork.ProductRepository.GetAllWithDetailsAsync();

            // Форматирование данных для DataTables
            var productDtos = products.Select(p => new {
				p.Id,
                p.Name,
                p.Description,
                p.Price,
                CategoryName = p.Category.Name,
                p.ImageUrl
            }).ToList();

            // Возвращаем данные в формате, ожидаемом DataTables
            return Json(new { data = productDtos });
        }

        #endregion
    }
}
