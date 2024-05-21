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
    [Authorize(Roles = $"{RoleConstants.Role_Company},{RoleConstants.Role_Admin}")]
    public class UserController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

		public IEnumerable<Company> Companies;
		private readonly IWebHostEnvironment webHostEnvironment;
        public UserController(StoreDbContext context, IWebHostEnvironment webHostEnv)
        {
            unitOfWork = new UnitOfWork(context);
			webHostEnvironment = webHostEnv;
        }
        public async Task<IActionResult> Index()
        {
            Companies = await unitOfWork.CompanyRepository.GetAllAsync();
            return View(Companies);
        }
		public IActionResult Create()
		{
			return View(new Company());
		}

		[HttpPost]
		public async Task<IActionResult> Create(Company company)
		{
			if (ModelState.IsValid)
			{
				await unitOfWork.CompanyRepository.AddAsync(company);
				await unitOfWork.SaveAsync();
				TempData["Success"] = "Company added successfully";
				return RedirectToAction("Index");
			}
			else
			{
				TempData["Error"] = "Company not added";
			}

			// Always return the view with the Company view model
			return View(company);
		}


		public async Task<IActionResult> Edit(int id)
		{
			if (id == 0)
			{
				return NotFound();
			}

			var company = await unitOfWork.CompanyRepository.GetByIdAsync(id);
			if (company == null)
			{
				return NotFound();
			}

			return View(company);
		}

		[HttpPost]
		public async Task<IActionResult> Edit(Company company)
		{
			if (ModelState.IsValid)
			{
				unitOfWork.CompanyRepository.Update(company);
				await unitOfWork.SaveAsync();
				TempData["Success"] = "Company updated successfully";
				return RedirectToAction("Index", "Company");
			}
			else
			{
				TempData["Error"] = "Company update failed";
				return View(company);
			}
		}
		public async Task<IActionResult> Delete(int id)
		{
			if (id == 0)
			{
				return NotFound();
			}
			var Company = await unitOfWork.CompanyRepository.GetByIdAsync(id);
			if (Company == null)
			{
				return NotFound();
			}
			return View(Company);
		}
		[HttpPost, ActionName("Delete")]
		public async Task<IActionResult> Delete(Company company)
		{
			unitOfWork.CompanyRepository.Delete(company);
			await unitOfWork.SaveAsync();
			TempData["Success"] = "Company deleted successfully";
			return RedirectToAction("Index", "Company");
		}


        #region API CALLS

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var Companies = await unitOfWork.CompanyRepository.GetAllAsync();

            return Json(new { data = Companies });
        }

        #endregion
    }
}
