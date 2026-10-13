using Domain.ValueObjects;

namespace Application.BulkPools.LockPool;

public record LockPoolCommand(PoolId PoolId);