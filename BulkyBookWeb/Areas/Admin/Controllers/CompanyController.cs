using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]    
    public class CompanyController : Controller
    {
        private readonly ICompanyRepository _companyRepository;

        public CompanyController(ICompanyRepository companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public async Task<IActionResult> Index()
        {
            await Task.CompletedTask;
            return View();
        }

        public async Task<IActionResult> Upsert(int? id)
        {
            var company = new Company();

            if (id == null || id == 0)
            {
                return View(company);
            }

            company = await _companyRepository.GetByIdAsync(x => x.Id == id);

            return View(company);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(Company company)
        {
            if (ModelState.IsValid)
            {
                if (company.Id == 0)
                {
                    await _companyRepository.InsertAsync(company);
                    TempData["success"] = "Company created successfully";
                }
                else
                {
                    await _companyRepository.UpdateAsync(company);
                    TempData["success"] = "Company updated successfully";
                }

                return RedirectToAction("Index");
            }
            return View(company);
        }

        #region API CALLS

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var companyList = await _companyRepository.GetAllAsync();
            return Json(new { data = companyList });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int? id)
        {
            var company = await _companyRepository.GetByIdAsync(x => x.Id == id);
            if (company == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            await _companyRepository.DeleteAsync(company);
            return Json(new { success = true, message = "Company delete successfully" });
        }

        #endregion
    }
}
