using Application.Interfaces;
using Domain;

namespace Application.Merchants.UpdateMerchantPhoneNumber;

public class UpdateMerchantPhoneNumberHandler(IMerchantRepository merchant, IUnitOfWork uow)
{
    private readonly IMerchantRepository _merchant = merchant;
    private readonly IUnitOfWork _uow = uow;

    public async Task<UpdateMerchantPhoneNumberResult> Handle(UpdateMerchantPhoneNumberCommand cmd, CancellationToken ct)
    {
        Merchant merchant_ = await _merchant.GetByIdAsync(cmd.MerchantId, ct) ?? throw new KeyNotFoundException("Merchant does not exist.");
        merchant_.UpdatePhoneNumber(cmd.PhoneNumber);
        _merchant.Update(merchant_);
        await _uow.SaveChangesAsync(ct);
        return new UpdateMerchantPhoneNumberResult(merchant_.Name, merchant_.PhoneNumber.Value);
    }
}