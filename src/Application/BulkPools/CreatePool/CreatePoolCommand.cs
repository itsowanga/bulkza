using Domain;
using Domain.ValueObjects;

namespace Application.BulkPools.CreatePool;
public record CreatePoolCommand(PoolId PoolId, ProductId ProductId, Quantity MinimumQuantity, int MinimumMerchants, DateTimeOffset Deadline);