using Application.Carts.AddCartItem;
using Application.Carts.ClearCart;
using Application.Carts.RemoveCartItem;
using Application.Carts.UpdateItemQuantity;
using Application.Carts.ViewCart;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Carts;

[ApiController]
[Route("api/carts")]
public class CartController( ClearCartHandler clear,
                             AddCartItemHandler add,
                             RemoveCartItemHandler remove,
                             UpdateItemQuantityHandler update,
                             ViewCartHandler view) : ControllerBase
{
    private readonly ClearCartHandler _clear = clear;
    private readonly AddCartItemHandler _add = add;
    private readonly RemoveCartItemHandler _remove = remove;
    private readonly UpdateItemQuantityHandler _update = update;
    private readonly ViewCartHandler _view = view;



    [Authorize]
    [HttpPost("items")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request, CancellationToken ct)
    {
        var merchantId = User.GetMerchantId();
        if (merchantId is null) return Unauthorized();

        AddCartItemCommand command = new(MerchantId.Create((Guid)merchantId), ProductId.Create(request.ProductId), Quantity.Create(request.Quantity));
        var result = await _add.Handle(command, ct);
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("items/{productId}")]
    public async Task<IActionResult> RemoveItem(Guid productId, CancellationToken ct)
    {
        var merchantId = User.GetMerchantId();
        if (merchantId is null) return Unauthorized();

        RemoveCartItemCommand command = new(MerchantId.Create((Guid)merchantId), ProductId.Create(productId));
        var result = await _remove.Handle(command, ct);
        return Ok(result);
    }

    [Authorize]
    [HttpPut("items/{productId}")]
    public async Task<IActionResult> UpdateQuantity(Guid productId, UpdateQuantityRequest request, CancellationToken ct)
    {
        var merchantId = User.GetMerchantId();
        if (merchantId is null) return Unauthorized();

        UpdateItemQuantityCommand command = new(MerchantId.Create((Guid)merchantId), ProductId.Create(productId), Quantity.Create(request.Quantity));
        var result = await _update.Handle(command, ct);
        return Ok(result);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> ViewCart(CancellationToken ct)
    {
        var merchantId = User.GetMerchantId();
        if (merchantId is null) return Unauthorized();

        ViewCartQuery query = new(MerchantId.Create((Guid)merchantId));
        var result = await _view.Handle(query, ct);
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("clear")]
    public async Task<IActionResult> ClearCart(CancellationToken ct)
    {
        var merchantId = User.GetMerchantId();
        if (merchantId is null) return Unauthorized();

        ClearCartCommand command = new(MerchantId.Create((Guid)merchantId));
        var result = await _clear.Handle(command, ct);
        return Ok(result); 
    }

}