using Application.Interfaces;
using Domain;

namespace Application.Merchants.CreateMerchant;

public class CreateMerchantHandler (IMerchantRepository merchant, IUnitOfWork uow)

{
    private readonly IMerchantRepository _merchant = merchant;
    private readonly IUnitOfWork _uow = uow;

    public async Task<CreateMerchantResult> Handle(CreateMerchantCommand cmd, CancellationToken ct)
    {
        Merchant merchant_ = new(cmd.MerchantId, cmd.Name, cmd.Status, cmd.Role, cmd.Address, cmd.PhoneNumber, cmd.RegistrationDate);
        await _merchant.AddAsync(merchant_, ct);
        await _uow.SaveChangesAsync(ct);
        return new CreateMerchantResult(cmd.MerchantId, cmd.Name, cmd.Status, cmd.RegistrationDate);
    }
}