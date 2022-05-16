using System.Security.Claims;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IShoppingCartRepository _shoppingCartRepository;

        public ShoppingCartViewComponent(IShoppingCartRepository shoppingCartRepository)
        {
            _shoppingCartRepository = shoppingCartRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claims = (ClaimsIdentity)User.Identity;
            var claim = claims?.FindFirst(ClaimTypes.NameIdentifier);

            if (claim != null)
            {
                if (HttpContext.Session.GetInt32(SD.SessionCart) != null)
                {
                    return View(HttpContext.Session.GetInt32(SD.SessionCart));
                }

                HttpContext.Session.SetInt32(SD.SessionCart, (await _shoppingCartRepository.GetAllAsync(x => x.ApplicationUserId == claim.Value)).ToList().Count);
                return View(HttpContext.Session.GetInt32(SD.SessionCart));
            }

            HttpContext.Session.Clear();
            return View(0);
        }
    }
}
