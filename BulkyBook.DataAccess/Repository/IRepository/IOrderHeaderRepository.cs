using BulkyBook.Models;

namespace BulkyBook.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository : IGenericRepository<OrderHeader>
    {
        Task UpdateStatus(int id, string orderStatus, string? paymentStatus = null);
        Task UpdateStripePaymentId(int id, string sessionId, string paymentIntentId);
    }
}
