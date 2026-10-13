using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Merchants.CreateMerchant;

public record CreateMerchantCommand(MerchantId MerchantId, string Name, MerchantStatus Status, MerchantRole Role,
                                    Address Address, PhoneNumber PhoneNumber, DateTimeOffset RegistrationDate); 