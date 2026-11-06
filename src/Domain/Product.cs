using Domain.Enums;
using Domain.ValueObjects;

namespace Domain;

public class Product
{
    public ProductId ProductId {get; private set;}
    public string SKU {get; init;}
    public string Name {get; init;}
    public string? Description {get; private set;}
    public ProductCategory Category {get; private set;}
    public UnitOfMeasure UnitOfMeasure {get; private set;}
    public Money BasePrice {get; private set;}
    private readonly List<DiscountTier> _discountTiers = [];
    public IReadOnlyList<DiscountTier> DiscountTiers => _discountTiers;
    public bool IsActive {get; private set;}


    public Product( ProductId productId,
                    string SKU,
                    string name,
                    string? description,
                    ProductCategory category,
                    UnitOfMeasure unitOfMeasure,
                    Money basePrice,
                    bool isActive)
    {

        if (productId == default) throw new ArgumentException($"{nameof(productId)} cannot be the default value.", nameof(productId));
        if (!Enum.IsDefined(category)) throw new ArgumentException($"{nameof(category)} is invalid.", nameof(category));
        if (!Enum.IsDefined(unitOfMeasure)) throw new ArgumentException($"{nameof(unitOfMeasure)} is invalid.", nameof(unitOfMeasure));
        
        ValidateOptional(description);
        ValidateRequired(SKU, "SKU", 50);
        ValidateRequired(name, "Name", 100);
        AddDiscountTiers(_discountTiers);
        ProductId = productId;
        Category = category;
        UnitOfMeasure = unitOfMeasure;
        BasePrice = basePrice;
        IsActive = isActive;
        this.SKU = SKU;
        Name = name;
        Description = description;
        

    }
    private static void ValidateOptional(string? description, int maxLength=500)
    {
        if (!string.IsNullOrWhiteSpace(description))
        {
            if (description.Length > maxLength) throw new ArgumentException($"{nameof(description)} cannot exceed {maxLength} characters.", nameof(description));
        }
    }
    private static void ValidateRequired(string value, string name, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException($"{name} is required.", nameof(value));    
        if (value.Length > maxLength) throw new ArgumentException($"{name} cannot exceed {maxLength} characters.", nameof(value));
    }

    private static void AddDiscountTiers(List<DiscountTier> tiers)
    {
        tiers.AddRange(
        [
            DiscountTier.Create(Quantity.Create(1), 0m),
            DiscountTier.Create(Quantity.Create(50), 5m),
            DiscountTier.Create(Quantity.Create(100), 10m),
            DiscountTier.Create(Quantity.Create(250), 15m),
            DiscountTier.Create(Quantity.Create(500), 20m),
            DiscountTier.Create(Quantity.Create(1000), 25m)
        ]);
    }
    public void UpdateDescription(string? description)
    {
        ValidateOptional(description);
        Description = description;
    }

    public void UpdateCategory(ProductCategory category)
    {
        if (!Enum.IsDefined(category)) throw new ArgumentException($"{nameof(category)} is invalid.", nameof(category));
        Category = category;
    }
    public void UpdateUnitOfMeasure(UnitOfMeasure unitOfMeasure)
    {
       if (!Enum.IsDefined(unitOfMeasure)) throw new ArgumentException($"{nameof(unitOfMeasure)} is invalid.", nameof(unitOfMeasure));
        UnitOfMeasure = unitOfMeasure; 
    }
    public void UpdateBasePrice(Money basePrice)
    {
        if (basePrice == default) throw new ArgumentException($"{nameof(basePrice)} cannot contain the default value.", nameof(basePrice));
        BasePrice = basePrice; 
    }
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

}