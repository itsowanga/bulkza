using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Tests.Builders;

public class BulkPoolBuilder
{
    private PoolId _poolId = PoolId.New();
    private ProductId _productId = ProductId.New();
    private Quantity _minimumQuantity = Quantity.Create(5);
    private int _minimumMerchants = 5;
    private DateTimeOffset _deadline = DateTimeOffset.UtcNow.AddMonths(1);

    public BulkPoolBuilder WithPoolId(PoolId poolId)
    {
        _poolId = poolId;
        return this;
    }

    public BulkPoolBuilder WithProductId(ProductId productId)
    {
        _productId = productId;
        return this;
    }

    public BulkPoolBuilder WithMinimumQuantity(Quantity minimumQuantity)
    {
        _minimumQuantity = minimumQuantity;
        return this;
    }

    public BulkPoolBuilder WithMinimumMerchants(int minimumMerchants)
    {
        _minimumMerchants = minimumMerchants;
        return this;
    }

    public BulkPoolBuilder WithDeadline(DateTimeOffset deadline)
    {
        _deadline = deadline;
        return this;
    }

    public BulkPool Build() => new(_poolId, _productId, _minimumQuantity, _minimumMerchants, _deadline);

}