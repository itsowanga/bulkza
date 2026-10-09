using System.Reflection.Metadata.Ecma335;
using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Tests.Builders;

public class ProductBuilder
{
    
    private ProductId _productId = ProductId.New();
    private string _sku = "abc";
    private string _name = "bacc";
    private string? _description = null;
    private ProductCategory _category = ProductCategory.Flour;
    private UnitOfMeasure _unitOfMeasure = UnitOfMeasure.Each;
    private Money _basePrice = Money.Create(55m);
    private bool _isActive = true;


    public ProductBuilder WithProductId(ProductId productId)
    {
        _productId = productId;
        return this;
    }

    public ProductBuilder WithSku(string sku)
    {
        _sku = sku;
        return this;
    }

    public ProductBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ProductBuilder WithProductDescription(string? description)
    {
        _description = description;
        return this;
    }

    public ProductBuilder WithProductCategory(ProductCategory category)
    {
        _category = category;
        return this;
    }

    public ProductBuilder WithUnitOfMeasure(UnitOfMeasure unitOfMeasure)
    {
        _unitOfMeasure = unitOfMeasure;
        return this;
    }

    public ProductBuilder WithBasePrice(Money basePrice)
    {
        _basePrice = basePrice;
        return this;
    }

    public ProductBuilder WithIsActive(bool isActive)
    {
        _isActive = isActive;
        return this;
    }

    public Product Build() => new(_productId, _sku, _name, _description, _category, _unitOfMeasure, _basePrice, _isActive);
}