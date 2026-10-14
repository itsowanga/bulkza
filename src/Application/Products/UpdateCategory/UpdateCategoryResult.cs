using Domain;
using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Products.UpdateCategory;

public record UpdateCategoryResult(ProductId ProductId, ProductCategory Category);