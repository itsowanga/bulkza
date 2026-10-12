using Domain.ValueObjects;

namespace Application.BulkPools.JoinPool;

public record JoinPoolCommand(PoolId PoolId, MerchantId MerchantId, Quantity Quantity);