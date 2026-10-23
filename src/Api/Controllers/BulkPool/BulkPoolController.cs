using System.Runtime.InteropServices;
using Application.BulkPools.CreatePool;
using Application.BulkPools.JoinPool;
using Application.BulkPools.LeavePool;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.BulkPool;

[ApiController]
[Route("api/bulkpools")]
public class BulkPoolController( CreatePoolHandler create, JoinPoolHandler join, LeavePoolHandler leave)
: ControllerBase
{
    private readonly CreatePoolHandler _create = create;
    private readonly JoinPoolHandler _join = join;
    private readonly LeavePoolHandler _leave = leave;

    [HttpPost]
    public async Task<IActionResult> CreatePool([FromBody] CreatePoolRequest request, CancellationToken ct)
    {
        CreatePoolCommand command = new(ProductId.Create(request.ProductId),
                                                         Quantity.Create(request.MinimumQuantity),
                                                         request.MinimumMerchants,
                                                         request.Deadline);

        var result = await _create.Handle(command, ct);
        return Ok(result);
    }

    [HttpPost("{id}/join")]
    public async Task<IActionResult> JoinPool(Guid id, [FromBody] JoinPoolRequest request, CancellationToken ct)
    {
        JoinPoolCommand command = new(PoolId.Create(id), MerchantId.Create(request.MerchantId), Quantity.Create(request.Quantity));
        var result = await _join.Handle(command, ct);
        return Ok(result);
    }

    [HttpPost("{id}/leave")]
    public async Task<IActionResult> LeavePool(Guid id, [FromBody] LeavePoolRequest request, CancellationToken ct)
    {
        LeavePoolCommand command = new(PoolId.Create(id), MerchantId.Create(request.MerchantId));
        var result = await _leave.Handle(command, ct);
        return Ok(result);
    }

}