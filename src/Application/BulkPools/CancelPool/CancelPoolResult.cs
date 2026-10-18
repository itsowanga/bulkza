using Domain.Enums;
using Domain.ValueObjects;

namespace Application.BulkPools.CancelPool;

public record CancelPoolResult(Guid PoolId, BulkPoolStatus Status);