using Application.Interfaces;
using Domain;

namespace Application.Carts.UpdateItemQuantity;

public class UpdateItemQuantityHandler(ICartRepository cart, IUnitOfWork uow)
{
    private readonly ICartRepository _cart = cart;
    private readonly IUnitOfWork _uow = uow;

    public async Task<UpdateItemQuantityResult> Handle(UpdateItemQuantityCommand cmd, CancellationToken ct)

    {
        Cart cart_ = await _cart.GetCartByIdAsync(cmd.CartId, ct) ?? throw new KeyNotFoundException("Cart does not exist.");
        cart_.UpdateQuantity(cmd.ProductId, cmd.Quantity);
        _cart.Update(cart_);
        await _uow.SaveChangesAsync(ct);
        return new(cart_.CartId, cart_.MerchantId, cart_.CartItems, cart_.GetTotalQuantity());
    } 
}