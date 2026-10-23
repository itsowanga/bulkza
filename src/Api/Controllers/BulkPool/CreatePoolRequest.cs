namespace Api.Controllers.BulkPool;

public record CreatePoolRequest(Guid ProductId, int MinimumQuantity, int MinimumMerchants, DateTimeOffset Deadline);