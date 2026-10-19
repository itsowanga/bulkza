using System.Data;
using Application.Interfaces;
using Domain;
using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Merchants.RegisterMerchant;

public class RegisterMerchantHandler (IMerchantRepository merchant, IPasswordHasher passwordHasher, IUnitOfWork uow)

{
    private readonly IMerchantRepository _merchant = merchant;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly IUnitOfWork _uow = uow;

    public async Task<RegisterMerchantResult> Handle(RegisterMerchantCommand cmd, CancellationToken ct)
    {
        var existingMerchant = await _merchant.GetByPhoneNumberAsync(cmd.PhoneNumber, ct);

        if (existingMerchant is not null)
        {
            throw new InvalidOperationException("Merchant already exists, try to login.");
        }

        Merchant merchant = new(MerchantId.New(), cmd.Name, PasswordHash.Create(_passwordHasher.Hash(cmd.Password.Password)), 
                                MerchantStatus.Active, MerchantRole.Merchant, cmd.Address, cmd.PhoneNumber, DateTimeOffset.UtcNow);

        await _merchant.AddAsync(merchant, ct);
        await _uow.SaveChangesAsync(ct);
        return new RegisterMerchantResult(merchant.Id.Value);
    }
}