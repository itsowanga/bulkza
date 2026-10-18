using Domain.Enums;
using Domain.ValueObjects;

namespace Application.BulkPools.LockPool;

public record LockPoolResult(Guid PoolId, BulkPoolStatus Status);