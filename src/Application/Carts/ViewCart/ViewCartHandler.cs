using System.Reflection.Metadata.Ecma335;
using Application.Interfaces;
using Domain.ValueObjects;

namespace Application.Carts.ViewCart;

public class ViewCartHandler(ICartRepository cart)
{
    private readonly ICartRepository _cart = cart;

    public async Task<CartDto> Handle(ViewCartQuery query, CancellationToken ct)
    {
        var cart = await _cart.GetByMerchantIdAsync(query.MerchantId, ct);

        if(cart is null)
        {
            return new(Guid.Empty, query.MerchantId.Value, [], 0);
        }

        var dtos =  cart.CartItems.Select(item => new CartItemDto(item.ProductId.Value, item.Quantity.Value)).ToList();  
        
        return new(cart.CartId.Value, cart.MerchantId.Value, dtos, cart.GetTotalQuantity());
    }
}