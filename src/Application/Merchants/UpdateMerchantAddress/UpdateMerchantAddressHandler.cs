using Application.Interfaces;
using Domain;

namespace Application.Merchants.UpdateMerchantAddress;

public class UpdateMerchantAddressHandler(IMerchantRepository merchant, IUnitOfWork uow)
{
    private readonly IMerchantRepository _merchant = merchant;
    private readonly IUnitOfWork _uow = uow;

    public async Task<UpdateMerchantAddressResult> Handle(UpdateMerchantAddressCommand cmd, CancellationToken ct)
    {
        Merchant merchant_ = await _merchant.GetByIdAsync(cmd.MerchantId, ct) ?? throw new KeyNotFoundException("Merchant does not exist.");
        merchant_.UpdateAddress(cmd.Address);
        _merchant.Update(merchant_);
        await _uow.SaveChangesAsync(ct);
        return new UpdateMerchantAddressResult(merchant_.Name, merchant_.Address);
    }
}