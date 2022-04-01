using System.Linq.Expressions;
using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.EntityFrameworkCore;

namespace BulkyBook.DataAccess.Repository
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public Repository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<TEntity> GetByIdAsync(int id)
        {
            var query = await _applicationDbContext.Set<TEntity>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

            return query;
        }

        public async Task<TEntity> GetByIdAsync(Expression<Func<TEntity, bool>> predicate)
        {
            var query = _applicationDbContext.Set<TEntity>().Where(predicate);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            var query = await _applicationDbContext.Set<TEntity>().AsNoTracking().ToListAsync();
            return query;
        }

        public async Task InsertAsync(TEntity entity)
        {
            await _applicationDbContext.Set<TEntity>().AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(TEntity entity)
        {
            _applicationDbContext.Set<TEntity>().Update(entity);
            await _applicationDbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(TEntity entity)
        {
            _applicationDbContext.Set<TEntity>().Remove(entity);
            await _applicationDbContext.SaveChangesAsync();
        }

        public async Task DeleteRangeAsync(IEnumerable<TEntity> entity)
        {
            _applicationDbContext.Set<TEntity>().RemoveRange(entity);
            await _applicationDbContext.SaveChangesAsync();
        }
    }
}
