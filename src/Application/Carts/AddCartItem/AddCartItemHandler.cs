using Application.Interfaces;
using Domain;
using Domain.ValueObjects;

namespace Application.Carts.AddCartItem;

public class AddCartItemHandler(ICartRepository cart, IUnitOfWork uow)
{
    private readonly ICartRepository _cart = cart;
    private readonly IUnitOfWork _uow = uow;

    public async Task<AddCartItemResult> Handle(AddCartItemCommand cmd, CancellationToken ct)

    {
        var cart = await _cart.GetByMerchantIdAsync(cmd.MerchantId, ct);
        var doesNotExist = cart is null;

        if (doesNotExist) cart = new Cart(CartId.New(), cmd.MerchantId);
        
        cart!.AddItem(cmd.ProductId, cmd.Quantity);

        if(doesNotExist) await _cart.AddAsync(cart, ct);
        else
        {
            _cart.Update(cart);
        }
        
        var dtos = cart.CartItems.Select(item => new CartItemDto(item.ProductId.Value, item.Quantity.Value)).ToList();
        await _uow.SaveChangesAsync(ct);
        return new(cart.CartId.Value, cart.MerchantId.Value, dtos, cart.GetTotalQuantity());
    } 
}