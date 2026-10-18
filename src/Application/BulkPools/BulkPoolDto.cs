using Domain.Enums;

namespace Application.BulkPools;
public record BulkPoolDto(Guid PoolId, Guid ProductId, BulkPoolStatus Status,
                          int CurrentQuantity, int MinimumQuantity, DateTimeOffset Deadline, DateTimeOffset CreatedAt
                          );