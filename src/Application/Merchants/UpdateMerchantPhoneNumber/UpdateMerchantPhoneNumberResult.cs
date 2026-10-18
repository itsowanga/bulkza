using Domain.ValueObjects;

namespace Application.Merchants.UpdateMerchantPhoneNumber;

public record UpdateMerchantPhoneNumberResult(string Name, string PhoneNumber);