using Domain;

namespace Application.Interfaces;
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid productId, CancellationToken ct = default);
    Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetActiveAsync(int page, int pageSize, CancellationToken ct = default);
    Task AddAsync(Product product, CancellationToken ct = default);
    void Update(Product product);
}