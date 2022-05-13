using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderHeaderRepository _orderHeaderRepository;

        public OrderController(IOrderHeaderRepository orderHeaderRepository)
        {
            _orderHeaderRepository = orderHeaderRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region API CALLS

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            IEnumerable<OrderHeader> orderHeaders = await _orderHeaderRepository.GetAllAsync(includeProperties: "ApplicationUser");
            return Json(new { data = orderHeaders });
        }

        #endregion
    }
}
