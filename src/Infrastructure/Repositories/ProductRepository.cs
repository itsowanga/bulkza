using Application.Interfaces;
using Domain;
using Domain.Enums;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ProductRepository(BulkzaDbContext db) : IProductRepository
{
    private readonly BulkzaDbContext _db = db;

    public Task<Product?> GetByIdAsync(ProductId productId, CancellationToken ct = default) => _db.Products.FirstOrDefaultAsync(product => product.ProductId == productId, ct);
    public Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default) => _db.Products.FirstOrDefaultAsync(product => product.SKU == sku, ct);
    public async Task<(IReadOnlyList<Product>, int)> GetByNameAsync(string? name, int page, int pageSize, CancellationToken ct = default) 
    {
        if (name is null)
        {
            return await GetActiveAsync(page, pageSize, null, ct);
        }
        int totalProducts = await _db.Products.Where(product => product.Name.StartsWith(name) && (product.IsActive == true)).CountAsync(ct);
        IReadOnlyList<Product> products = await _db.Products.Where(product => product.Name.StartsWith(name) && (product.IsActive == true)).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        
        return (products, totalProducts);
    }
    public async Task<(IReadOnlyList<Product>, int)> GetActiveAsync(int page, int pageSize, ProductCategory? category ,CancellationToken ct = default)
    {
        if (category is null)
        {
            int totalProducts_ = await _db.Products.Where(product => product.IsActive).CountAsync(ct);
            IReadOnlyList<Product> products_ = await _db.Products.Where(product => product.IsActive).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
            return (products_, totalProducts_);
        }

        int totalProducts = await _db.Products.Where(product => product.IsActive && (product.Category == category)).CountAsync(ct);
        IReadOnlyList<Product> products = await _db.Products.Where(product => product.IsActive && (product.Category == category)).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (products, totalProducts);
    }

    public async Task AddAsync(Product product, CancellationToken ct = default) => await _db.Products.AddAsync(product, ct);
    public void Update(Product product) => _db.Products.Update(product);
}