using Application.Interfaces;
using Domain;

namespace Application.Carts.RemoveCartItem;

public class RemoveCartItemHandler(ICartRepository cart, IUnitOfWork uow)
{
    private readonly ICartRepository _cart = cart;
    private readonly IUnitOfWork _uow = uow;

    public async Task<RemoveCartItemResult> Handle(RemoveCartItemCommand cmd, CancellationToken ct)

    {
        Cart cart_ = await _cart.GetByMerchantIdAsync(cmd.MerchantId, ct) ?? throw new KeyNotFoundException("Cart does not exist.");
        cart_.RemoveItem(cmd.ProductId);
        var dtos = cart_.CartItems.Select(item => new CartItemDto(item.ProductId.Value, item.Quantity.Value)).ToList();
        _cart.Update(cart_);
        await _uow.SaveChangesAsync(ct);
        return new(cart_.CartId.Value, cart_.MerchantId.Value, dtos, cart_.GetTotalQuantity());
    } 
}