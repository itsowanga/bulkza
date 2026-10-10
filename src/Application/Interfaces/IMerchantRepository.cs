using Domain;
using Domain.ValueObjects;

namespace Application.Interfaces;
public interface IMerchantRepository
{
    Task<Merchant?> GetByIdAsync(Guid merchantId, CancellationToken ct = default);
    Task<Merchant?> GetByPhoneNumberAsync(PhoneNumber phoneNumber, CancellationToken ct = default);
    Task AddAsync(Merchant merchant, CancellationToken ct = default);
    Task<bool> ExistsAsync(PhoneNumber phoneNumber, CancellationToken ct = default);
    void Update(Merchant merchant);

}