using Domain.Enums;

namespace Application.BulkPools.JoinPool;
public record JoinPoolResult(BulkPoolStatus Status, int CurrentQuantity);