using System.Security.Claims;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IShoppingCartRepository _shoppingCartRepository;
        private readonly IApplicationUserRepository _applicationUserRepository;
        private readonly IOrderHeaderRepository _orderHeaderRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;
        private readonly IEmailSender _emailSender;
        public ShoppingCartViewModel ShoppingCartViewModel { get; set; }

        public CartController(IShoppingCartRepository shoppingCartService,
            IApplicationUserRepository userService,
            IOrderHeaderRepository orderHeaderService,
            IOrderDetailRepository orderDetailService,
            IEmailSender emailSender)
        {
            _shoppingCartRepository = shoppingCartService;
            _applicationUserRepository = userService;
            _orderHeaderRepository = orderHeaderService;
            _orderDetailRepository = orderDetailService;
            _emailSender = emailSender;
        }

        public async Task<IActionResult> Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartViewModel = new ShoppingCartViewModel()
            {
                ListCart = await _shoppingCartRepository.GetAllAsync(u => u.ApplicationUserId == claim.Value, "Product"),
                OrderHeader = new OrderHeader()
            };
            foreach (var cart in ShoppingCartViewModel.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                ShoppingCartViewModel.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartViewModel);
        }

        public async Task<IActionResult> Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartViewModel = new ShoppingCartViewModel()
            {
                ListCart = await _shoppingCartRepository.GetAllAsync(u => u.ApplicationUserId == claim.Value, "Product"),
                OrderHeader = new OrderHeader()
            };

            ShoppingCartViewModel.OrderHeader.ApplicationUser = await _applicationUserRepository.GetByIdAsync(x => x.Id == claim.Value);

            ShoppingCartViewModel.OrderHeader.Name = ShoppingCartViewModel.OrderHeader.ApplicationUser.Name;
            ShoppingCartViewModel.OrderHeader.PhoneNumber = ShoppingCartViewModel.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartViewModel.OrderHeader.StreetAddress = ShoppingCartViewModel.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartViewModel.OrderHeader.City = ShoppingCartViewModel.OrderHeader.ApplicationUser.City;
            ShoppingCartViewModel.OrderHeader.State = ShoppingCartViewModel.OrderHeader.ApplicationUser.State;
            ShoppingCartViewModel.OrderHeader.PostalCode = ShoppingCartViewModel.OrderHeader.ApplicationUser.PostalCode;


            foreach (var cart in ShoppingCartViewModel.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                ShoppingCartViewModel.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Summary(ShoppingCartViewModel shoppingCartViewModel)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);

            shoppingCartViewModel.ListCart = await _shoppingCartRepository.GetAllAsync(u => u.ApplicationUserId == claim.Value, "Product");

            shoppingCartViewModel.OrderHeader.OrderDate = DateTime.Now;
            shoppingCartViewModel.OrderHeader.ApplicationUserId = claim.Value;

            foreach (var cart in shoppingCartViewModel.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                shoppingCartViewModel.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            ApplicationUser applicationUser = await _applicationUserRepository.GetByIdAsync(x => x.Id == claim.Value);
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                shoppingCartViewModel.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                shoppingCartViewModel.OrderHeader.OrderStatus = SD.PaymentStatusPending;
            }
            else
            {
                shoppingCartViewModel.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                shoppingCartViewModel.OrderHeader.OrderStatus = SD.StatusApproved;
            }

            await _orderHeaderRepository.InsertAsync(shoppingCartViewModel.OrderHeader);


            foreach (var cart in shoppingCartViewModel.ListCart)
            {
                OrderDetail orderDetail = new OrderDetail
                {
                    ProductId = cart.ProductId,
                    OrderId = shoppingCartViewModel.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };

                await _orderDetailRepository.InsertAsync(orderDetail);
            }

            #region Stripe Payment

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                var host = "https://localhost:7193";
                var options = new SessionCreateOptions
                {
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                    SuccessUrl = $"{host}/Customer/Cart/OrderConfirmation?id={shoppingCartViewModel.OrderHeader.Id}",
                    CancelUrl = $"{host}/Customer/Cart/Index"
                };

                foreach (var item in shoppingCartViewModel.ListCart)
                {
                    var sessionLineItemOptions = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100), // 20.00 -> 2000
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title
                            },
                        },
                        Quantity = item.Count
                    };

                    options.LineItems.Add(sessionLineItemOptions);
                }

                var service = new SessionService();
                Session session = await service.CreateAsync(options);

                await _orderHeaderRepository.UpdateStripePaymentId(shoppingCartViewModel.OrderHeader.Id, session.Id,
                    session.PaymentIntentId);

                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }

            return RedirectToAction("OrderConfirmation", "Cart", new { id = shoppingCartViewModel.OrderHeader.Id });

            #endregion
        }

        public async Task<IActionResult> OrderConfirmation(int id)
        {
            OrderHeader orderHeader = await _orderHeaderRepository.GetByIdAsync(x => x.Id == id, "ApplicationUser");
            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                //check the stripe status
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    await _orderHeaderRepository.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                }
            }
            await _emailSender.SendEmailAsync(orderHeader.ApplicationUser.Email, "New Order - Bulky Book", "<p>New Order Created.</p>");
            List<ShoppingCart> shoppingCarts = (await _shoppingCartRepository.GetAllAsync(x => x.ApplicationUserId == orderHeader.ApplicationUserId)).ToList();
            HttpContext.Session.Clear();
            await _shoppingCartRepository.DeleteRangeAsync(shoppingCarts);

            return View(id);
        }

        public async Task<IActionResult> Plus(int cartId)
        {
            var cart = await _shoppingCartRepository.GetByIdAsync(x => x.Id == cartId);
            _shoppingCartRepository.IncrementCount(cart, 1);
            await _shoppingCartRepository.UpdateAsync(cart);

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Minus(int cartId)
        {
            var cart = await _shoppingCartRepository.GetByIdAsync(x => x.Id == cartId);
            if (cart.Count <= 1)
            {
                await _shoppingCartRepository.DeleteAsync(cart);
                var count = (await _shoppingCartRepository.GetAllAsync(x => x.ApplicationUserId == cart.ApplicationUserId)).ToList().Count;
                HttpContext.Session.SetInt32(SD.SessionCart, count);
            }
            else
            {
                _shoppingCartRepository.DecrementCount(cart, 1);
                await _shoppingCartRepository.UpdateAsync(cart);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Remove(int cartId)
        {
            var cart = await _shoppingCartRepository.GetByIdAsync(x => x.Id == cartId);
            await _shoppingCartRepository.DeleteAsync(cart);

            var count = (await _shoppingCartRepository.GetAllAsync(x => x.ApplicationUserId == cart.ApplicationUserId)).ToList().Count;
            HttpContext.Session.SetInt32(SD.SessionCart, count);

            return RedirectToAction(nameof(Index));
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
