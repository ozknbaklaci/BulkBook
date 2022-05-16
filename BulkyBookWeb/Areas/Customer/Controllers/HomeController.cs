using System.Diagnostics;
using System.Security.Claims;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductRepository _productRepository;
        private readonly IShoppingCartRepository _shoppingCartRepository;
        public HomeController(ILogger<HomeController> logger,
            IProductRepository productRepository,
            IShoppingCartRepository shoppingCartRepository)
        {
            _logger = logger;
            _productRepository = productRepository;
            _shoppingCartRepository = shoppingCartRepository;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<Product> productList = await _productRepository.GetAllAsync(includeProperties: "Category,CoverType");
            return View(productList);
        }

        public async Task<IActionResult> Details(int productId)
        {
            ShoppingCart shoppingCart = new()
            {
                Count = 1,
                ProductId = productId,
                Product = await _productRepository.GetByIdAsync(x => x.Id == productId, "Category,CoverType")
            };

            return View(shoppingCart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);
            shoppingCart.ApplicationUserId = claim?.Value;

            ShoppingCart cartFromDb = await _shoppingCartRepository.GetByIdAsync(x => x.ApplicationUserId == shoppingCart.ApplicationUserId && x.ProductId == shoppingCart.ProductId);

            if (cartFromDb == null)
            {
                await _shoppingCartRepository.InsertAsync(shoppingCart);
                HttpContext.Session.SetInt32(SD.SessionCart, (await _shoppingCartRepository.GetAllAsync(x => x.ApplicationUserId == claim.Value)).ToList().Count);
            }
            else
            {
                _shoppingCartRepository.IncrementCount(cartFromDb, shoppingCart.Count);
                await _shoppingCartRepository.UpdateAsync(cartFromDb);
            }

            return RedirectToAction(nameof(Index));
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