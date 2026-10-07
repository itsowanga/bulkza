using Domain.Enums;
using Domain.ValueObjects;

namespace Domain;

public class Product
{
    public ProductId ProductId {get; private set;}
    public string SKU {get; private set;}
    public string Name {get; private set;}
    public string? Description {get; private set;}
    public ProductCategory Category {get; private set;}
    public UnitOfMeasure UnitOfMeasure {get; private set;}
    public Money BasePrice {get; private set;}
    public bool IsActive {get; private set;}


    public Product( ProductId productId,
                    string sku,
                    string name,
                    string? description,
                    ProductCategory productCategory,
                    UnitOfMeasure unitOfMeasure,
                    Money basePrice,
                    bool isActive)
    {
        
        if (!Enum.IsDefined(productCategory))
        {
            throw new ArgumentException("Invalid Product Category.");
        }

        if (!Enum.IsDefined(unitOfMeasure))
        {
            throw new ArgumentException("Invalid Unit of Measure.");
        }

        ValidateOptional(description);
        ValidateRequired(sku, "SKU", 50);
        ValidateRequired(name, "Name", 100);
        
        ProductId = productId;
        Category = productCategory;
        UnitOfMeasure = unitOfMeasure;
        BasePrice = basePrice;
        IsActive = isActive;
        SKU = sku;
        Name = name;
        Description = description;
    }

    private void ValidateOptional(string? description, int maxLength=500)
    {
        if (!string.IsNullOrWhiteSpace(description))
        {
            if (description.Length > maxLength)
            {
                throw new ArgumentException($"Description cannot exceed {maxLength} characters.");

            }
        }
    }

    private void ValidateRequired(string value, string name, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{name} is required.");
        }
        if (value.Length > maxLength)
        {
            throw new ArgumentException($"{name} cannot exceed {maxLength} characters.");
    
        }
    }


}