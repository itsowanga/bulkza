using Domain.ValueObjects;

namespace Application.Merchants.UpdateMerchantName;
public record UpdateMerchantNameResult(Guid MerchantId, string Name);