using Domain.Enums;
using Domain.ValueObjects;

namespace Application.BulkPools.ExpirePool;
public record ExpirePoolResult(Guid PoolId, BulkPoolStatus Status);