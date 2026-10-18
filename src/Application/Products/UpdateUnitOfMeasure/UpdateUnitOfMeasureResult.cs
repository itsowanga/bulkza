using Domain;
using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Products.UpdateUnitOfMeasure;

public record UpdateUnitOfMeasureResult(Guid ProductId, UnitOfMeasure UnitOfMeasure);