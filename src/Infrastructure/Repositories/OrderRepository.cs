using Application.Interfaces;
using Domain;
using Domain.Enums;
using Domain.ValueObjects;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;
public class OrderRepository(BulkzaDbContext db) : IOrderRepository
{
    private readonly BulkzaDbContext _db = db;

    public async Task<Order?> GetOrderByIdAsync(OrderId orderId, CancellationToken ct = default) => await _db.Orders.FirstOrDefaultAsync(order => order.OrderId == orderId, ct);
    public async Task<(IReadOnlyList<Order>, int)>  GetByMerchantIdAsync(MerchantId merchantId, int page, int pageSize, CancellationToken ct = default) 
    {
        int totalOrders =  await _db.Orders.Where(order => order.MerchantId == merchantId).CountAsync(ct);
        IReadOnlyList<Order> orders = await _db.Orders.Where(order => order.MerchantId == merchantId).OrderByDescending(order => order.CreatedAt).ThenBy(order => order.OrderId).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (orders, totalOrders);
       
    }
    public async Task<(IReadOnlyList<Order>, int)> GetAllAsync(OrderStatus? status, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.Orders.Where(order => status == null || order.Status == status);
        int total = await query.CountAsync(ct);
        IReadOnlyList<Order> orders = await query.OrderByDescending(order => order.CreatedAt).ThenBy(order => order.OrderId)
                                                 .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (orders, total);
    }

    public async Task<IReadOnlyDictionary<OrderStatus, int>> CountByStatusAsync(CancellationToken ct = default) =>
        await _db.Orders.GroupBy(order => order.Status).Select(group => new { group.Key, Count = group.Count() })
                        .ToDictionaryAsync(row => row.Key, row => row.Count, ct);

    public async Task AddAsync(Order order, CancellationToken ct = default) => await _db.Orders.AddAsync(order, ct);
    public void Update(Order order) => _db.Orders.Update(order);
    public async Task<IReadOnlyList<Order>> GetByStatusAsync(OrderStatus status, CancellationToken ct = default) => await _db.Orders
                                                                                                .Where(order => order.Status == status).ToListAsync(ct);

}