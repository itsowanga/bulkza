using Domain.ValueObjects;

namespace Domain;

public class Product
{
    public ProductId ProductId {get; private set;}
    public string SKU {get; private set;}
    public string Name {get; private set;}
    public string Description {get; private set;}
    
}