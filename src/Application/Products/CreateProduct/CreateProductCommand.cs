using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Products.CreateProduct;

public record CreateProductCommand(string Name, string? Description, ProductCategory ProductCategory, 
                                   UnitOfMeasure UnitOfMeasure, Money BasePrice);

                                   