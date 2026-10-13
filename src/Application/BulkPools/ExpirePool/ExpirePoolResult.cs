using Domain.Enums;
using Domain.ValueObjects;

namespace Application.BulkPools.ExpirePool;
public record ExpirePoolResult(PoolId PoolId, BulkPoolStatus Status);