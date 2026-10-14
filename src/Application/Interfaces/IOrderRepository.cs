using Domain;
using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Interfaces;
public interface IOrderRepository
{
    Task<Order?> GetOrderByIdAsync(OrderId orderId, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> GetByMerchantIdAsync(MerchantId merchantId, CancellationToken ct = default);
    Task AddAsync(Order order, CancellationToken ct = default);
    void Update(Order order);
    Task<IReadOnlyList<Order>> GetByStatusAsync(OrderStatus status, CancellationToken ct = default);

}