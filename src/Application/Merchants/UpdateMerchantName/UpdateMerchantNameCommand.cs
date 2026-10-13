using Domain;
using Domain.ValueObjects;

namespace Application.Merchants.UpdateMerchantName;
public record UpdateMerchantNameCommand(MerchantId MerchantId, string Name);