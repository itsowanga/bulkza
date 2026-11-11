using Domain;
using Domain.Enums;
using Domain.ValueObjects;

namespace Application.BulkPools;

public record BulkPoolDto(Guid PoolId, Guid ProductId, BulkPoolStatus Status,
                          int CurrentQuantity, int MyQuantity, DateTimeOffset Deadline,
                          int MinimumQuantity, int MinimumMerchants, int ParticipantCount)
{
    /// <summary>Builds the DTO from the point of view of <paramref name="viewer"/>, who sees their own quantity in the pool.</summary>
    public static BulkPoolDto From(BulkPool pool, MerchantId viewer)
    {
        var participant = pool.Participants.FirstOrDefault(p => p.MerchantId == viewer);
        var myQuantity = participant != default ? participant.Quantity.Value : 0;

        return new BulkPoolDto(pool.PoolId.Value, pool.ProductId.Value, pool.Status,
                               pool.CurrentQuantity, myQuantity, pool.Deadline,
                               pool.MinimumQuantity.Value, pool.MinimumMerchants, pool.Participants.Count);
    }
}
