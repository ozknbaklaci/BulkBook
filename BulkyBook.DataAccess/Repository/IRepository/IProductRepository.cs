﻿using BulkyBook.Models;

namespace BulkyBook.DataAccess.Repository.IRepository
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task UpdateProductAsync(Product product);
    }
}
