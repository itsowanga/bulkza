using Application.BulkPools.FinalisePool;
using Application.Interfaces;
using Domain;
using Domain.ValueObjects;

namespace Application.BulkPools.FinalisePool;
public class FinalisePoolHandler(IBulkPoolRepository bulkPool, IProductRepository product, IOrderRepository order, IUnitOfWork uow)
{
    private readonly IBulkPoolRepository _bulkPool = bulkPool;
    private readonly IProductRepository _product = product;
    private readonly IOrderRepository _order = order;
    private readonly IUnitOfWork _uow = uow;

    public async Task<FinalisePoolResult> Handle(FinalisePoolCommand cmd, CancellationToken ct)
    {
        BulkPool pool = await _bulkPool.GetByIdAsync(cmd.PoolId, ct) ?? throw new KeyNotFoundException("Pool does not exist.");
        var product = await _product.GetByIdAsync(pool.ProductId, ct) ?? throw new KeyNotFoundException("Product does not exist.");
        var discountedPrice = new VolumePricingStrategy(product.DiscountTiers).CalculateUnitPrice(product.BasePrice, pool.CurrentQuantity);
        pool.Finalise();
        
        foreach (Participant p in pool.Participants)
        {
            OrderLine orderLine = OrderLine.Create(pool.ProductId, p.Quantity, discountedPrice);
            List<OrderLine> orderLines = [orderLine];
            await _order.AddAsync(new(OrderId.Create(Guid.NewGuid()), p.MerchantId, orderLines), ct);
        }

        await _uow.SaveChangesAsync(ct);
        return new FinalisePoolResult(pool.PoolId.Value, pool.Status);
    }
}