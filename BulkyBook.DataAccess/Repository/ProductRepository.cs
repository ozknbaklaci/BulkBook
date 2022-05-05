using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.EntityFrameworkCore;

namespace BulkyBook.DataAccess.Repository
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        public ProductRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task UpdateProductAsync(Product product)
        {
            var productFromDb = await _applicationDbContext.Products.FirstOrDefaultAsync(m => m.Id == product.Id);
            if (productFromDb != null)
            {
                productFromDb.Title = product.Title;
                productFromDb.ISBN = product.ISBN;
                productFromDb.Price = product.Price;
                productFromDb.Price50 = product.Price50;
                productFromDb.Price100 = product.Price100;
                productFromDb.ListPrice = product.ListPrice;
                productFromDb.Description = product.Description;
                productFromDb.CategoryId = product.CategoryId;
                productFromDb.Author = product.Author;
                productFromDb.CoverTypeId = product.CoverTypeId;

                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    productFromDb.ImageUrl = product.ImageUrl;
                }

                _applicationDbContext.Update(productFromDb);
                await _applicationDbContext.SaveChangesAsync();
            }
        }
    }
}
