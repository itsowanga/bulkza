using Domain;
using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Products.UpdateUnitOfMeasure;

public record UpdateUnitOfMeasureResult(ProductId ProductId, UnitOfMeasure UnitOfMeasure);