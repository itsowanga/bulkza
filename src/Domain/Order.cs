using Domain.Enums;
using Domain.ValueObjects;

namespace Domain;
public class Order
{
    public OrderId OrderId {get; private set;}
    public MerchantId MerchantId {get; private set;}
    public OrderStatus Status {get; private set;}
    public Money TotalAmount {get; private set;}
    public DateTimeOffset CreatedAt {get; private set;}
    public Guid? PaymentId {get; private set;}
    public Guid? RefundId {get; private set;}
    private List<OrderLine> _orderLines;

    public Order( OrderId orderId,
                  MerchantId merchantId,
                  IReadOnlyList<OrderLine> orderLines
                )
    {
        if (orderId == default)
        {
            throw new ArgumentException($"{nameof(orderId)} cannot be the default value.");
        }
        if (merchantId == default)
        {
            throw new ArgumentException($"{nameof(merchantId)} cannot be the default value.");
        }
    
        OrderId = orderId;
        MerchantId = merchantId;
        Status = OrderStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
        PaymentId = null;
        RefundId = null;

        ArgumentNullException.ThrowIfNull(orderLines);

        if (orderLines.Count == 0)
        {
            throw new ArgumentException($"{nameof(orderLines)} must have at least one line.");
        }

        _orderLines = [.. orderLines];
        TotalAmount = Money.Create(_orderLines.Sum(l => l.UnitPrice.Amount * l.Quantity.Value));
    }

    public IReadOnlyList<OrderLine> OrderLines => _orderLines;
    public void MarkAsPaid(Guid paymentId)
    {
        if (Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException($"{nameof(Status)} should be pending.");
        }

        if (paymentId == default)
        {
            throw new ArgumentException($"{nameof(paymentId)} cannot be empty when order is paid", nameof(paymentId));
        }

        Status = OrderStatus.Paid;
        PaymentId = paymentId;
    }

    public void MarkAsShipped()
    {
        if (Status != OrderStatus.Paid)
        {
            throw new InvalidOperationException($"{nameof(Status)} should be paid.");
        }

        Status = OrderStatus.Shipped;     
    }

    public void MarkAsDelivered()
    {
        if (Status != OrderStatus.Shipped)
        {
            throw new InvalidOperationException($"{nameof(Status)} should be shipped.");
        }

        Status = OrderStatus.Delivered;     
    }

    public void Cancel(Guid refundId)
    {
        if(Status != OrderStatus.Pending && Status != OrderStatus.Paid)
        {
            throw new InvalidOperationException($"Order cannot be cancelled.");
        }

        if(refundId == default)
        {
            throw new ArgumentException($"{nameof(refundId)} cannot be empty when order is cancelled.", nameof(refundId));
        }

        Status = OrderStatus.Cancelled;
        RefundId = refundId;
    }


}