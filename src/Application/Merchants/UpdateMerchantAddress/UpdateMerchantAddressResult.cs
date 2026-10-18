using Domain.ValueObjects;

namespace Application.Merchants.UpdateMerchantAddress;


public record UpdateMerchantAddressResult(string Name, AddressDto Address);

