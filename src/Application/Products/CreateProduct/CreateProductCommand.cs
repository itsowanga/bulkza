using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Products.CreateProduct;

public record CreateProductCommand(ProductId ProductId, string Sku, string Name, string? Description, ProductCategory ProductCategory, 
                                   UnitOfMeasure UnitOfMeasure, Money BasePrice, bool IsActive 
                                   );

                                   