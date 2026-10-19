using Domain.ValueObjects;

namespace Application.BulkPools.SearchPool.SearchOpenByProductId;
public record SearchOpenByProductIdQuery(ProductId ProductId, MerchantId MerchantId, int Page, int PageSize);