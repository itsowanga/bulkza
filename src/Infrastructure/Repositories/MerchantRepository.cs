using Application.Interfaces;
using Domain;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class MerchantRepository(BulkzaDbContext db) : IMerchantRepository
{
    private readonly BulkzaDbContext _db = db;

    public Task<Merchant?> GetByIdAsync(MerchantId merchantId, CancellationToken ct = default) => _db.Merchants.FirstOrDefaultAsync(merchant => merchant.Id == merchantId, ct);
    public Task<Merchant?> GetByPhoneNumberAsync(PhoneNumber phoneNumber, CancellationToken ct = default) => _db.Merchants.FirstOrDefaultAsync(merchant => merchant.PhoneNumber == phoneNumber, ct);
    public async Task AddAsync(Merchant merchant, CancellationToken ct = default) => await _db.Merchants.AddAsync(merchant, ct);
    public async Task<bool> ExistsAsync(PhoneNumber phoneNumber, CancellationToken ct = default) => await _db.Merchants.AnyAsync(merchant => merchant.PhoneNumber == phoneNumber, ct);
    public void Update(Merchant merchant) => _db.Merchants.Update(merchant);
}