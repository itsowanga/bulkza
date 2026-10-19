using Domain.ValueObjects;

namespace Application.Merchants.LoginMerchant;

public record LoginMerchantCommand(PhoneNumber PhoneNumber, string PasswordHash);