using Application.Admin.GetSummary;
using Application.Admin.ListOrders;
using Application.Admin.ListPools;
using Domain.Enums;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Admin;

/// <summary>Read-only views across every merchant. Admins only: anyone else gets 403, a missing token gets 401.</summary>
[ApiController]
[Route("api/admin")]
[Authorize(Roles = nameof(MerchantRole.Admin))]
public class AdminController(ListPoolsHandler listPools, ListOrdersHandler listOrders, GetSummaryHandler getSummary) : ControllerBase
{
    private readonly ListPoolsHandler _listPools = listPools;
    private readonly ListOrdersHandler _listOrders = listOrders;
    private readonly GetSummaryHandler _getSummary = getSummary;

    [HttpGet("pools")]
    public async Task<IActionResult> ListPools([FromQuery] BulkPoolStatus? status, [FromQuery] int page, [FromQuery] int pageSize, CancellationToken ct)
    {
        var merchantId = User.GetMerchantId();
        if (merchantId is null) return Unauthorized();

        var result = await _listPools.Handle(new(MerchantId.Create((Guid)merchantId), status, page, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("orders")]
    public async Task<IActionResult> ListOrders([FromQuery] OrderStatus? status, [FromQuery] int page, [FromQuery] int pageSize, CancellationToken ct)
    {
        var result = await _listOrders.Handle(new(status, page, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken ct)
    {
        var result = await _getSummary.Handle(ct);
        return Ok(result);
    }
}
