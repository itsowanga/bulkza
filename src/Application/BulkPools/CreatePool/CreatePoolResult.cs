using Domain.Enums;
using Domain.ValueObjects;

namespace Application.BulkPools.CreatePool;

public record CreatePoolResult(Guid PoolId, BulkPoolStatus Status, DateTimeOffset CreatedAt);