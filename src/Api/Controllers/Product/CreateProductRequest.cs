using Domain.Enums;

namespace Api.Controllers.Product;

public record CreateProductRequest(string Name, string? Description, ProductCategory Category, UnitOfMeasure UnitOfMeasure, 
                                    decimal BasePrice);