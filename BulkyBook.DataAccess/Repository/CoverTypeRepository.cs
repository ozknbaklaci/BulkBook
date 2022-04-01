using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;

namespace BulkyBook.DataAccess.Repository
{
    public class CoverTypeRepository : GenericRepository<CoverType>, ICoverTypeRepository
    {
        public CoverTypeRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
        }
    }
}
