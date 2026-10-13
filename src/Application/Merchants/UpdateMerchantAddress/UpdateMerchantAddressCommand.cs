using Domain.ValueObjects;

namespace Application.Merchants.UpdateMerchantAddress;

public record UpdateMerchantAddressCommand(MerchantId MerchantId, Address Address);