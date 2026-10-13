using Domain.Enums;
using Domain.ValueObjects;

namespace Application.BulkPools.FinalisePool;

public record FinalisePoolResult(PoolId PoolId, BulkPoolStatus Status);