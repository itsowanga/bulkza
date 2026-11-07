using Application.Orders.CancelOrder;
using Application.Orders.MarkAsDelivered;
using Application.Orders.MarkAsShipped;
using Application.Orders.SearchOrders.FindAllOrderByMerchant;
using Application.PaymentGateway.ProcessPayment;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Orders;

[ApiController]
[Route("api/orders")]
public class OrdersController ( ProcessPaymentHandler pay,
                                CancelOrderHandler cancel,
                                MarkAsShippedHandler ship, 
                                MarkAsDeliveredHandler deliver,
                                FindAllOrderByMerchantHandler findAll) : ControllerBase
{

    private readonly ProcessPaymentHandler _pay = pay;
    private readonly CancelOrderHandler _cancel = cancel;
    private readonly MarkAsShippedHandler _ship = ship;
    private readonly MarkAsDeliveredHandler _deliver = deliver;
    private readonly FindAllOrderByMerchantHandler _findAll = findAll;


    [Authorize]
    [HttpPost("{id}/pay")]
    public async Task<IActionResult> Pay(Guid id, [FromBody] PayOrderRequest request, CancellationToken ct)
    {
        var merchantId = User.GetMerchantId();
        if (merchantId is null) return Unauthorized();

        ProcessPaymentCommand command = new(OrderId.Create(id), MerchantId.Create((Guid)merchantId), request.CardToken);
        var result = await _pay.Handle(command, ct);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var merchantId = User.GetMerchantId();
        if (merchantId is null) return Unauthorized();

        CancelOrderCommand command = new(OrderId.Create(id), MerchantId.Create((Guid)merchantId));
        var result = await _cancel.Handle(command, ct);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id}/ship")]
    public async Task<IActionResult> Ship(Guid id, CancellationToken ct)
    {

        var merchantRole = User.GetMerchantRole();
        if ((merchantRole is null) || (merchantRole != Domain.Enums.MerchantRole.Admin)) return Forbid();
        
        MarkAsShippedCommand command = new(OrderId.Create(id));
        var result = await _ship.Handle(command, ct);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id}/deliver")]
    public async Task<IActionResult> Deliver(Guid id, CancellationToken ct)
    {
        var merchantRole = User.GetMerchantRole();
        if ((merchantRole is null) || (merchantRole != Domain.Enums.MerchantRole.Admin)) return Forbid();

        MarkAsDeliveredCommand command = new(OrderId.Create(id));
        var result = await _deliver.Handle(command, ct);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("mine")]
    public async Task<IActionResult> FindAll([FromQuery] int page, int pageSize , CancellationToken ct)
    {
        var merchantId = User.GetMerchantId();
        if(merchantId is null) return Unauthorized();

        FindAllOrderByMerchantQuery query = new(MerchantId.Create((Guid)merchantId), page, pageSize);
        var result = await _findAll.Handle(query, ct);
        return Ok(result);
    }

}
