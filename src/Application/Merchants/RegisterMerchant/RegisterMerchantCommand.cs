using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Merchants.RegisterMerchant;

public record RegisterMerchantCommand(string Name, PasswordHash Password, Address Address, PhoneNumber PhoneNumber); 