using Domain;
using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Products.UpdateCategory;

public record UpdateCategoryCommand(ProductId ProductId, ProductCategory Category);