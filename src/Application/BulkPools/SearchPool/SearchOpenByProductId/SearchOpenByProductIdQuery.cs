using Domain.ValueObjects;

namespace Application.BulkPools.SearchPool.SearchOpenByProductId;
public record SearchOpenByProductIdQuery(ProductId ProductId, int Page, int PageSize);