using Application.Interfaces;
using Domain;

namespace Application.Merchants.UpdateMerchantName;
public class UpdateMerchantNameHandler(IMerchantRepository merchant, IUnitOfWork uow)
{
    private readonly IMerchantRepository _merchant = merchant;
    private readonly IUnitOfWork _uow = uow;

    public async Task<UpdateMerchantNameResult> Handle(UpdateMerchantNameCommand cmd, CancellationToken ct)
    {
        Merchant merchant_ = await _merchant.GetByIdAsync(cmd.MerchantId, ct) ?? throw new KeyNotFoundException("Merchant does not exist.");
        merchant_.UpdateName(cmd.Name);
        _merchant.Update(merchant_);
        await _uow.SaveChangesAsync(ct);
        return new UpdateMerchantNameResult(merchant_.Id.Value, merchant_.Name);
    }
}