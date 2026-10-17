using Application.Interfaces;
using Domain;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ProductRepository(BulkzaDbContext db) : IProductRepository
{
    private readonly BulkzaDbContext _db = db;

    public Task<Product?> GetByIdAsync(ProductId productId, CancellationToken ct = default) => _db.Products.FirstOrDefaultAsync(product => product.ProductId == productId, ct);
    public Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default) => _db.Products.FirstOrDefaultAsync(product => product.SKU == sku, ct);
    public async Task<IReadOnlyList<Product>> GetActiveAsync(int page, int pageSize, CancellationToken ct = default) => await _db.Products.Where(product => product.IsActive == true)
                                                                                                    .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

    public async Task AddAsync(Product product, CancellationToken ct = default) => await _db.Products.AddAsync(product, ct);
    public void Update(Product product) => _db.Products.Update(product);
}