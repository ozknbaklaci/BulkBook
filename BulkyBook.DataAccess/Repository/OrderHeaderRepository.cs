using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.EntityFrameworkCore;

namespace BulkyBook.DataAccess.Repository
{
    public class OrderHeaderRepository : GenericRepository<OrderHeader>, IOrderHeaderRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public OrderHeaderRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            var orderFromDb = await _applicationDbContext.OrderHeaders.FirstOrDefaultAsync(x => x.Id == id);
            if (orderFromDb != null)
            {
                orderFromDb.OrderStatus = orderStatus;
                if (paymentStatus != null)
                {
                    orderFromDb.PaymentStatus = paymentStatus;
                }
                _applicationDbContext.Update(orderFromDb);
                await _applicationDbContext.SaveChangesAsync();
            }
        }

        public async Task UpdateStripePaymentId(int id, string sessionId, string paymentIntentId)
        {
            var orderFromDb = await _applicationDbContext.OrderHeaders.FirstOrDefaultAsync(x => x.Id == id);
            if (orderFromDb != null)
            {
                orderFromDb.PaymentDate = DateTime.Now;
                orderFromDb.SessionId = sessionId;
                orderFromDb.PaymentIntentId = paymentIntentId;

                _applicationDbContext.Update(orderFromDb);
                await _applicationDbContext.SaveChangesAsync();
            }
        }
    }
}
