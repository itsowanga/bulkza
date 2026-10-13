using Domain.ValueObjects;

namespace Application.Merchants.UpdateMerchantPhoneNumber;

public record UpdateMerchantPhoneNumberCommand(MerchantId MerchantId, PhoneNumber PhoneNumber);