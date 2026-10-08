using Domain.ValueObjects;
namespace Domain;
public interface IPricingStrategy
{
    public Money CalculateUnitPrice(Money basePrice, int quantity);
}