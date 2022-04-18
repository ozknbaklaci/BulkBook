using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICoverTypeRepository _coverTypeRepository;
        private readonly IWebHostEnvironment _hostEnvironment;


        public ProductController(IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            ICoverTypeRepository coverTypeRepository,
            IWebHostEnvironment hostEnvironment)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _coverTypeRepository = coverTypeRepository;
            _hostEnvironment = hostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }


        public async Task<IActionResult> Upsert(int? id)
        {
            var categoryList = await _categoryRepository.GetAllAsync();
            var categoryListItems = categoryList.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });

            var coverTypeList = await _coverTypeRepository.GetAllAsync();

            var coverTypeListItem = coverTypeList.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });

            var productViewModel = new ProductViewModel
            {
                Product = new Product(),
                CategoryList = categoryListItems,
                CoverTypeList = coverTypeListItem
            };


            if (id == null || id == 0)
            {
                //create product
                //ViewBag.CategoryList = categoryListItems;
                //ViewData["CoverTypeList"] = coverTypeListItem;
                return View(productViewModel);
            }

            productViewModel.Product = await _productRepository.GetByIdAsync(x => x.Id == id);

            return View(productViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(ProductViewModel productViewModel, IFormFile file)
        {

            if (ModelState.IsValid)
            {
                string wwwRootPath = _hostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid() + "_" + file.FileName;
                    string uploads = Path.Combine(wwwRootPath + @"\images\products");
                    string filePath = Path.Combine(uploads, fileName);

                    await using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    productViewModel.Product.ImageUrl = @"\images\products\" + fileName;
                }
                await _productRepository.InsertAsync(productViewModel.Product);
                TempData["success"] = "Product created successfully";
                return RedirectToAction("Index");
            }
            return View(productViewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var productFromDb = await _productRepository.GetByIdAsync(x => x.Id == id);

            return View(productFromDb);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int? id)
        {
            var productFromDb = await _productRepository.GetByIdAsync(x => x.Id == id);

            await _productRepository.DeleteAsync(productFromDb);
            TempData["success"] = "Product deleted successfully";
            return RedirectToAction("Index");

        }

        #region API CALLS

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productRepository.GetAllAsync("Category,CoverType");
            return Json(new { data = products });
        }

        #endregion
    }
}
