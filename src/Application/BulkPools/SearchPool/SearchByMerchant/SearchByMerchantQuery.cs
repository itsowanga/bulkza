using Domain.ValueObjects;

namespace Application.BulkPools.SearchPool.SearchByMerchant;

public record SearchByMerchantQuery(MerchantId MerchantId, int Page, int PageSize);
