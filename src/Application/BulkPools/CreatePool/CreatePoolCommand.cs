using Domain;
using Domain.ValueObjects;

namespace Application.BulkPools.CreatePool;
public record CreatePoolCommand(MerchantId MerchantId, Quantity MerchantQuantity, ProductId ProductId, Quantity MinimumQuantity, int MinimumMerchants, DateTimeOffset Deadline);