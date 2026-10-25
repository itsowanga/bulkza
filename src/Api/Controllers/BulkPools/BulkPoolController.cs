using Application.BulkPools.CancelPool;
using Application.BulkPools.CreatePool;
using Application.BulkPools.ExpirePool;
using Application.BulkPools.FinalisePool;
using Application.BulkPools.JoinPool;
using Application.BulkPools.LeavePool;
using Application.BulkPools.LockPool;
using Application.BulkPools.SearchPool.SearchByMerchant;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.BulkPools;

[ApiController]
[Route("api/bulkpools")]
public class BulkPoolsController(CreatePoolHandler create, 
                                 JoinPoolHandler join, 
                                 LeavePoolHandler leave,
                                 LockPoolHandler lock_,
                                 FinalisePoolHandler finalise,
                                 ExpirePoolHandler expire,
                                 CancelPoolHandler cancel,
                                 SearchByMerchantHandler searchMine)
: ControllerBase
{
    private readonly CreatePoolHandler _create = create;
    private readonly JoinPoolHandler _join = join;
    private readonly LeavePoolHandler _leave = leave;
    private readonly LockPoolHandler _lock = lock_;
    private readonly FinalisePoolHandler _finalise = finalise;
    private readonly ExpirePoolHandler _expire = expire;
    private readonly CancelPoolHandler _cancel = cancel;
    private readonly SearchByMerchantHandler _searchMine = searchMine;

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
    [HttpGet("mine")]
    public async Task<IActionResult> GetMyPools([FromQuery] int page, [FromQuery] int pageSize, CancellationToken ct)
    {
        var merchantId = User.GetMerchantId();
        if (merchantId is null) return Unauthorized();

        SearchByMerchantQuery query = new(MerchantId.Create((Guid)merchantId), page, pageSize);
        var result = await _searchMine.Handle(query, ct);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id}/join")]
    public async Task<IActionResult> JoinPool(Guid id, [FromBody] JoinPoolRequest request, CancellationToken ct)
    {
        var merchantId = User.GetMerchantId();
        if (merchantId is null) return Unauthorized();

        JoinPoolCommand command = new(PoolId.Create(id), MerchantId.Create((Guid)merchantId), Quantity.Create(request.Quantity));
        var result = await _join.Handle(command, ct);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id}/leave")]
    public async Task<IActionResult> LeavePool(Guid id,  CancellationToken ct)
    {
        var merchantId = User.GetMerchantId();
        if (merchantId is null) return Unauthorized();

        LeavePoolCommand command = new(PoolId.Create(id), MerchantId.Create((Guid)merchantId));
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