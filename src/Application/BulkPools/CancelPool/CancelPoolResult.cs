using Domain.Enums;
using Domain.ValueObjects;

namespace Application.BulkPools.CancelPool;

public record CancelPoolResult(PoolId PoolId, BulkPoolStatus Status);