using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CoverTypeController : Controller
    {
        private readonly ICoverTypeRepository _coverTypeRepository;

        public CoverTypeController(ICoverTypeRepository coverTypeRepository)
        {
            _coverTypeRepository = coverTypeRepository;
        }

        public async Task<IActionResult> Index()
        {
            var coverTypeList = await _coverTypeRepository.GetAllAsync();
            return View(coverTypeList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CoverType coverType)
        {
            if (ModelState.IsValid)
            {
                await _coverTypeRepository.InsertAsync(coverType);
                TempData["success"] = "CoverType created successfully";

                return RedirectToAction("Index");
            }

            return View(coverType);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var coverTypeFromDb = await _coverTypeRepository.GetByIdAsync(x => x.Id == id);

            return View(coverTypeFromDb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CoverType coverType)
        {

            if (ModelState.IsValid)
            {
                await _coverTypeRepository.UpdateAsync(coverType);
                TempData["success"] = "CoverType updated successfully";
                return RedirectToAction("Index");
            }
            return View(coverType);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var coverTypeFromDb = await _coverTypeRepository.GetByIdAsync(x => x.Id == id);

            return View(coverTypeFromDb);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int? id)
        {
            var coverTypeFromDb = await _coverTypeRepository.GetByIdAsync(x => x.Id == id);

            await _coverTypeRepository.DeleteAsync(coverTypeFromDb);
            TempData["success"] = "CoverType deleted successfully";
            return RedirectToAction("Index");

        }
    }
}
