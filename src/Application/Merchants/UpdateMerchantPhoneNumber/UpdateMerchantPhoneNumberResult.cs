using Domain.ValueObjects;

namespace Application.Merchants.UpdateMerchantPhoneNumber;

public record UpdateMerchantPhoneNumberResult(String Name, PhoneNumber PhoneNumber);