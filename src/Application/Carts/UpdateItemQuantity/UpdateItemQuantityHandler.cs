using Application.Interfaces;
using Domain;

namespace Application.Carts.UpdateItemQuantity;

public class UpdateItemQuantityHandler(ICartRepository cart, IUnitOfWork uow)
{
    private readonly ICartRepository _cart = cart;
    private readonly IUnitOfWork _uow = uow;

    public async Task<UpdateItemQuantityResult> Handle(UpdateItemQuantityCommand cmd, CancellationToken ct)

    {
        Cart cart_ = await _cart.GetByMerchantIdAsync(cmd.MerchantId, ct) ?? throw new KeyNotFoundException("Cart is empty.");
        cart_.UpdateQuantity(cmd.ProductId, cmd.Quantity);
        var dtos = cart_.CartItems.Select(item => new CartItemDto(item.ProductId.Value, item.Quantity.Value)).ToList();
        _cart.Update(cart_);
        await _uow.SaveChangesAsync(ct);
        return new(cart_.CartId.Value, cart_.MerchantId.Value, dtos, cart_.GetTotalQuantity());
    } 
}