using Domain.ValueObjects;

namespace Api.Controllers.BulkPool;

public record JoinPoolRequest(Guid PoolId, Guid MerchantId, int Quantity);