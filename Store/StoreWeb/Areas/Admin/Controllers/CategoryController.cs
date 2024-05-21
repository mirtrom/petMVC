using Microsoft.AspNetCore.Mvc;
using StoreDataAccess.Models.Data;
using StoreDataAccess.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using StoreDataAccess.Interfaces;
using Microsoft.AspNetCore.Hosting;
using StoreDataAccess.ViewModels;
using Microsoft.AspNetCore.Authorization;
using StoreDataAccess.Constants;

namespace StoreWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
	[Authorize(Roles = RoleConstants.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        public IEnumerable<Category> Categories;
		private readonly IWebHostEnvironment webHostEnvironment;
		public CategoryController(StoreDbContext context, IWebHostEnvironment webHostEnv)
		{
			unitOfWork = new UnitOfWork(context);
			webHostEnvironment = webHostEnv;
		}
        public async Task<IActionResult> Index()
        {
            Categories = await unitOfWork.CategoryRepository.GetAllAsync();
            return View(Categories);
        }

        public IActionResult Create()
        {
            return View();
        }
		[HttpPost]
		public async Task<IActionResult> Create(Category category, IFormFile file)
		{
			if (ModelState.IsValid)
			{
				if (file != null)
				{
					string wwwRootPath = webHostEnvironment.WebRootPath;
					string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
					string categoryPath = Path.Combine(wwwRootPath, @"images\category");
					using (var fileStream = new FileStream(Path.Combine(categoryPath, fileName), FileMode.Create))
					{
						file.CopyTo(fileStream);
					}
					category.ImageUrl = @"images\category\" + fileName;
				}

				await unitOfWork.CategoryRepository.AddAsync(category);
				await unitOfWork.SaveAsync();
				TempData["Success"] = "Category added successfully";
				return RedirectToAction("Index");
			}
			else
			{
				TempData["Error"] = "Category not added";
				return View();
			}
		}

		public async Task<IActionResult> Edit(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }
            var category = await unitOfWork.CategoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }
		[HttpPost]
		public async Task<IActionResult> Edit(Category category, IFormFile file)
		{
			if (ModelState.IsValid)
			{
				string wwwRootPath = webHostEnvironment.WebRootPath;
				if (file != null)
				{
					// Deleting the old image if it exists
					if (!string.IsNullOrEmpty(category.ImageUrl))
					{
						var oldImagePath = Path.Combine(wwwRootPath, category.ImageUrl.TrimStart('\\'));
						if (System.IO.File.Exists(oldImagePath))
						{
							System.IO.File.Delete(oldImagePath);
						}
					}

					// Uploading the new image
					string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
					string productPath = Path.Combine(wwwRootPath, @"images\category");
					using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
					{
						file.CopyTo(fileStream);
					}
					category.ImageUrl = @"images\category\" + fileName;
				}
				unitOfWork.CategoryRepository.Update(category);
				await unitOfWork.SaveAsync();
				TempData["Success"] = "Category updated successfully";
				return RedirectToAction("Index", "Category");
			}
			return View(category);
		}
		public async Task<IActionResult> Delete(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }
            var category = await unitOfWork.CategoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> Delete(Category category)
        {
            unitOfWork.CategoryRepository.Delete(category);
            await unitOfWork.SaveAsync();
            TempData["Success"] = "Category deleted successfully";
            return RedirectToAction("Index", "Category");
        }
    }
}
