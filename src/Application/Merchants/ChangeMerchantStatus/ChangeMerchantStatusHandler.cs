using Application.Interfaces;
using Domain;

namespace Application.Merchants.ChangeMerchantStatus;

public class ChangeMerchantStatusHandler(IMerchantRepository merchant, IUnitOfWork uow)
{
    private readonly IMerchantRepository _merchant = merchant;
    private readonly IUnitOfWork _uow = uow;

    public async Task<ChangeMerchantStatusResult> Handle(ChangeMerchantStatusCommand cmd, CancellationToken ct)
    {
        Merchant merchant_ = await _merchant.GetByIdAsync(cmd.MerchantId, ct) ?? throw new KeyNotFoundException("Merchant does not exist.");
        merchant_.ChangeMerchantStatus(cmd.Status);
        _merchant.Update(merchant_);
        await _uow.SaveChangesAsync(ct);
        return new ChangeMerchantStatusResult(merchant_.Name, merchant_.Status);
    }
}