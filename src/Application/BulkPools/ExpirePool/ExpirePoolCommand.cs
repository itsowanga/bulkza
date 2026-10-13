using Domain.ValueObjects;

namespace Application.BulkPools.ExpirePool;

public record ExpirePoolCommand(PoolId PoolId);