using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Merchants.CreateMerchant;

public record CreateMerchantResult(MerchantId MerchantId, string Name, MerchantStatus Status, DateTimeOffset RegistrationDate);