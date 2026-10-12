using Domain.Enums;

namespace Application.BulkPools.LeavePool;
public record LeavePoolResult(BulkPoolStatus Status, int CurrentQuantity);