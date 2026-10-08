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

        if (orderLines is null)
        {
            throw new ArgumentNullException(nameof(orderLines));
        }

        if (orderLines.Count == 0)
        {
            throw new ArgumentException($"{nameof(orderLines)} must have at least one line.");
        }

        _orderLines = new List<OrderLine>(orderLines);
        TotalAmount = Money.Create(_orderLines.Sum(l => l.UnitPrice.Amount * l.Quantity.Value));
    }

    public IReadOnlyList<OrderLine> OrderLines => _orderLines;
    public void MarkAsPaid()
    {
        if (Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException($"{nameof(Status)} should be pending.");
        }

        Status = OrderStatus.Paid;
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

    public void Cancel()
    {
        if(Status != OrderStatus.Pending && Status != OrderStatus.Paid)
        {
            throw new InvalidOperationException($"Order cannot be cancelled.");
        }

        Status = OrderStatus.Cancelled;
    }


}