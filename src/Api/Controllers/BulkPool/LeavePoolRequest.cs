using Domain.ValueObjects;

namespace Api.Controllers.BulkPool;

public record LeavePoolRequest(Guid MerchantId);