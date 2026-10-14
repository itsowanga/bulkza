using Domain;
using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Products.UpdateUnitOfMeasure;

public record UpdateUnitOfMeasureCommand(ProductId ProductId, UnitOfMeasure UnitOfMeasure);