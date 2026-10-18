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
        AddressDto dto = new(merchant_.Address.Street,
                            merchant_.Address.City,
                            merchant_.Address.Province,
                            merchant_.Address.PostalCode,
                            merchant_.Address.Country,
                            merchant_.Address.Suburb);
        _merchant.Update(merchant_);
        await _uow.SaveChangesAsync(ct);
        return new UpdateMerchantAddressResult(merchant_.Name, dto);
    }
}