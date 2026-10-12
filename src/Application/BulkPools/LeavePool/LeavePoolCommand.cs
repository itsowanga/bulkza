using Domain.ValueObjects;

namespace Application.BulkPools.LeavePool;
public record LeavePoolCommand(PoolId PoolId, MerchantId MerchantId);