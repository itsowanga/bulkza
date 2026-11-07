using Domain;
using Domain.ValueObjects;

namespace Application.Interfaces;
public interface IMerchantRepository
{
    Task<Merchant?> GetByIdAsync(MerchantId merchantId, CancellationToken ct = default);
    Task<Merchant?> GetByPhoneNumberAsync(PhoneNumber phoneNumber, CancellationToken ct = default);
    /// <summary>Business names for a set of merchants, keyed by merchant id.</summary>
    Task<IReadOnlyDictionary<Guid, string>> GetNamesAsync(IReadOnlyCollection<MerchantId> merchantIds, CancellationToken ct = default);
    Task AddAsync(Merchant merchant, CancellationToken ct = default);
    Task<bool> ExistsAsync(PhoneNumber phoneNumber, CancellationToken ct = default);
    void Update(Merchant merchant);

}