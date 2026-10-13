using Domain.Events;
using Domain.ValueObjects;

namespace Application.BulkPools.CancelPool;
public record CancelPoolCommand(PoolId PoolId);