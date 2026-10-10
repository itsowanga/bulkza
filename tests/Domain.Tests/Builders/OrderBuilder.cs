using Domain.ValueObjects;

namespace Domain.Tests.Builders;
public class OrderBuilder
{
    private OrderId _orderId = OrderId.New();
    private MerchantId _merchantId = MerchantId.New();
    private IReadOnlyList<OrderLine> _orderLines = [OrderLine.Create(ProductId.New(), Quantity.Create(5), Money.Create(55m))];


    public OrderBuilder WithOrderId(OrderId orderId)
    {
        _orderId = orderId;
        return this;
    }

    public OrderBuilder WithMerchantId(MerchantId merchantId)
    {
        _merchantId = merchantId;
        return this;
    }

    public OrderBuilder WithOrderLines(IReadOnlyList<OrderLine> orderLines)
    {
        _orderLines = orderLines;
        return this;
    }

    public Order Build() => new(_orderId, _merchantId, _orderLines);
    
}