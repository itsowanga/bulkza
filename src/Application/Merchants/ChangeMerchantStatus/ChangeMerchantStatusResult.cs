using Domain.Enums;

namespace Application.Merchants.ChangeMerchantStatus;

public record ChangeMerchantStatusResult(string Name, MerchantStatus Status);