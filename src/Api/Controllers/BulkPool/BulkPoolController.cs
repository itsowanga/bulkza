using System.Runtime.InteropServices;
using Application.BulkPools.CancelPool;
using Application.BulkPools.CreatePool;
using Application.BulkPools.ExpirePool;
using Application.BulkPools.FinalisePool;
using Application.BulkPools.JoinPool;
using Application.BulkPools.LeavePool;
using Application.BulkPools.LockPool;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.BulkPool;

[ApiController]
[Route("api/bulkpools")]
public class BulkPoolController( CreatePoolHandler create, 
                                 JoinPoolHandler join, 
                                 LeavePoolHandler leave,
                                 LockPoolHandler lock_,
                                 FinalisePoolHandler finalise,
                                 ExpirePoolHandler expire,
                                 CancelPoolHandler cancel)
: ControllerBase
{
    private readonly CreatePoolHandler _create = create;
    private readonly JoinPoolHandler _join = join;
    private readonly LeavePoolHandler _leave = leave;
    private readonly LockPoolHandler _lock = lock_;
    private readonly FinalisePoolHandler _finalise = finalise;
    private readonly ExpirePoolHandler _expire = expire;
    private readonly CancelPoolHandler _cancel = cancel;

    [Authorize]
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

    [Authorize]
    [HttpPost("{id}/join/{merchantId}")]
    public async Task<IActionResult> JoinPool(Guid id, Guid merchantId, [FromBody] JoinPoolRequest request, CancellationToken ct)
    {
        JoinPoolCommand command = new(PoolId.Create(id), MerchantId.Create(merchantId), Quantity.Create(request.Quantity));
        var result = await _join.Handle(command, ct);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id}/leave/{merchantId}")]
    public async Task<IActionResult> LeavePool(Guid id, Guid merchantId, CancellationToken ct)
    {
        LeavePoolCommand command = new(PoolId.Create(id), MerchantId.Create(merchantId));
        var result = await _leave.Handle(command, ct);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id}/lock")]
    public async Task<IActionResult> LockPool(Guid id, CancellationToken ct)
    {
        LockPoolCommand command = new(PoolId.Create(id));
        var result = await _lock.Handle(command, ct);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id}/finalise")]
    public async Task<IActionResult> FinalisePool(Guid id, CancellationToken ct)
    {
        FinalisePoolCommand command = new(PoolId.Create(id));
        var result = await _finalise.Handle(command, ct);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id}/expire")]
    public async Task<IActionResult> ExpirePool(Guid id, CancellationToken ct)
    {
        ExpirePoolCommand command = new(PoolId.Create(id));
        var result = await _expire.Handle(command, ct);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelPool(Guid id, CancellationToken ct)
    {
        CancelPoolCommand command = new(PoolId.Create(id));
        var result = await _cancel.Handle(command, ct);
        return Ok(result);
    }




}