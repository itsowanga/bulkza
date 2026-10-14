using System.Diagnostics.CodeAnalysis;
using Application.Interfaces;
using Domain;

namespace Application.Carts.CreateCart;

public class CreateCartHandler(ICartRepository cart, IUnitOfWork uow)
{
    private readonly ICartRepository _cart = cart;
    private readonly IUnitOfWork _uow = uow;

    public async Task<CreateCartResult> Handle(CreateCartCommand cmd, CancellationToken ct)
    {
        Cart cart_ = new(cmd.CartId, cmd.MerchantId);
        await _cart.AddAsync(cart_, ct);
        await _uow.SaveChangesAsync(ct);
        return new CreateCartResult(cart_.CartId, cart_.MerchantId, cart_.CartItems, cart_.GetTotalQuantity());
    }
}