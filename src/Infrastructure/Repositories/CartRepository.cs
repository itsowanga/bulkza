using Application.Interfaces;
using Domain.ValueObjects;
using Domain;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;
public class CartRepository(BulkzaDbContext db) : ICartRepository
{
    private readonly BulkzaDbContext _db = db;

    public Task<Cart?> GetCartByIdAsync(CartId cartId, CancellationToken ct = default) => _db.Carts.FirstOrDefaultAsync(cart => cart.CartId == cartId, ct);
    public Task<Cart?> GetByMerchantIdAsync(MerchantId merchantId, CancellationToken ct = default) => _db.Carts.FirstOrDefaultAsync(cart => cart.MerchantId == merchantId, ct);
    public async Task AddAsync(Cart cart, CancellationToken ct = default) => await _db.Carts.AddAsync(cart, ct);
    public void Update(Cart cart) => _db.Carts.Update(cart);
    public void Remove(Cart cart) => _db.Carts.Remove(cart);

}