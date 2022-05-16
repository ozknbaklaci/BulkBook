using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
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
            await Task.CompletedTask;
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
                return View(productViewModel);
            }

            //update product            
            productViewModel.Product = await _productRepository.GetByIdAsync(x => x.Id == id);

            return View(productViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(ProductViewModel productViewModel, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                var wwwRootPath = _hostEnvironment.WebRootPath;

                if (file != null)
                {
                    var fileName = Guid.NewGuid() + "_" + file.FileName;
                    var uploads = Path.Combine(wwwRootPath + @"\images\products");
                    var filePath = Path.Combine(uploads, fileName);

                    if (productViewModel?.Product.ImageUrl != null)
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, productViewModel.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    await using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    if (productViewModel != null)
                        productViewModel.Product.ImageUrl = @"\images\products\" + fileName;
                }

                if (productViewModel?.Product.Id == 0)
                {
                    await _productRepository.InsertAsync(productViewModel.Product);
                    TempData["success"] = "Product created successfully";
                }
                else
                {
                    await _productRepository.UpdateProductAsync(productViewModel.Product);
                    TempData["success"] = "Product updated successfully";
                }

                return RedirectToAction("Index");
            }
            return View(productViewModel);
        }

        #region API CALLS

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productRepository.GetAllAsync(includeProperties: "Category,CoverType");
            return Json(new { data = products });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int? id)
        {
            var productFromDb = await _productRepository.GetByIdAsync(x => x.Id == id);
            if (productFromDb == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, productFromDb.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            await _productRepository.DeleteAsync(productFromDb);
            return Json(new { success = true, message = "Product delete successfully" });
        }

        #endregion
    }
}
