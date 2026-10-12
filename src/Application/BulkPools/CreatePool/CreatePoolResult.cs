using Domain.Enums;
using Domain.ValueObjects;

namespace Application.BulkPools.CreatePool;

public record CreatePoolResult(PoolId PoolId, BulkPoolStatus Status, DateTimeOffset CreatedAt);