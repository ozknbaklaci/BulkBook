using System.Security.Claims;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IShoppingCartRepository _shoppingCartService;
        public ShoppingCartViewModel ShoppingCartViewModel { get; set; }

        public CartController(IShoppingCartRepository shoppingCartService)
        {
            _shoppingCartService = shoppingCartService;
        }

        public async Task<IActionResult> Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartViewModel = new ShoppingCartViewModel()
            {
                ListCart = await _shoppingCartService.GetAllAsync(u => u.ApplicationUserId == claim.Value, "Product")
            };
            foreach (var cart in ShoppingCartViewModel.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                ShoppingCartViewModel.CartTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartViewModel);
        }

        private double GetPriceBasedOnQuantity(double quantity, double price, double price50, double price100)
        {
            if (quantity < 50)
            {
                return price;
            }

            return quantity < 100 ? price50 : price100;
        }
    }
}
