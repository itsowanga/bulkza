using Domain;
using Domain.ValueObjects;

namespace Application.Interfaces;
public interface ICartRepository
{
    Task<Cart?> GetCartByIdAsync(CartId cartId, CancellationToken ct = default);
    Task<Cart?> GetByMerchantIdAsync(MerchantId merchantId, CancellationToken ct = default);
    Task AddAsync(Cart cart, CancellationToken ct = default);
    void Update(Cart cart);
    void Remove(Cart cart);
}