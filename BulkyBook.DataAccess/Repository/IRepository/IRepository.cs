using System.Linq.Expressions;
using BulkyBook.Models;

namespace BulkyBook.DataAccess.Repository.IRepository
{
    public interface IRepository<TEntity> where TEntity : BaseEntity
    {
        Task<TEntity> GetByIdAsync(int id);
        Task<TEntity> GetByIdAsync(Expression<Func<TEntity, bool>> predicate);
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task InsertAsync(TEntity entity);
        Task UpdateAsync(TEntity entity);
        Task DeleteAsync(TEntity entity);
        Task DeleteRangeAsync(IEnumerable<TEntity> entity);
    }
}
