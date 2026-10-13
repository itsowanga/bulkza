using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Merchants.ChangeMerchantStatus;

public record ChangeMerchantStatusCommand(MerchantId MerchantId, MerchantStatus Status);