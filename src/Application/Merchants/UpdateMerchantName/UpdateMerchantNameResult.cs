using Domain.ValueObjects;

namespace Application.Merchants.UpdateMerchantName;
public record UpdateMerchantNameResult(MerchantId MerchantId, string Name);