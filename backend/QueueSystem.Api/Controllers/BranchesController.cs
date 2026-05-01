using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QueueSystem.Api.Auth;
using QueueSystem.Api.Data;
using QueueSystem.Api.Dtos;
using QueueSystem.Api.Models;
using QueueSystem.Api.Services;

namespace QueueSystem.Api.Controllers;

[ApiController]
[Route("api/branches/{branchId:int}")]
[Authorize(Policy = "FloorStaff")]
[ServiceFilter(typeof(BranchRouteMatchesClaimFilter))]
public class BranchesController : ControllerBase
{
    private readonly IQueueOrchestrator _queue;
    private readonly AppDbContext _db;

    public BranchesController(IQueueOrchestrator queue, AppDbContext db)
    {
        _queue = queue;
        _db = db;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<BranchDashboardDto>> Dashboard(int branchId, CancellationToken ct)
    {
        try
        {
            return Ok(await _queue.GetDashboardAsync(branchId, ct));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("info")]
    public async Task<ActionResult<BranchDetailDto>> BranchInfo(int branchId, CancellationToken ct)
    {
        var b = await _db.Branches.AsNoTracking().FirstOrDefaultAsync(x => x.Id == branchId, ct);
        if (b == null) return NotFound();
        return Ok(ToDetailDto(b));
    }

    [HttpPatch("settings")]
    [Authorize(Policy = "ManagerOnly")]
    public async Task<ActionResult<BranchDetailDto>> PatchBranchSettings(int branchId, [FromBody] PatchBranchSettingsRequest body, CancellationToken ct)
    {
        var b = await _db.Branches.FirstOrDefaultAsync(x => x.Id == branchId, ct);
        if (b == null) return NotFound();

        if (body.Name != null) b.Name = body.Name.Trim();
        if (body.Location != null) b.Location = body.Location.Trim();
        if (body.MaxCapacity.HasValue) b.MaxCapacity = Math.Max(1, body.MaxCapacity.Value);

        var m = body.CrowdMediumStartsAtPercent ?? b.CrowdMediumStartsAtPercent;
        var h = body.CrowdHighStartsAtPercent ?? b.CrowdHighStartsAtPercent;
        var o = body.OvercrowdStartsAtPercent ?? b.OvercrowdStartsAtPercent;
        if (body.CrowdMediumStartsAtPercent.HasValue || body.CrowdHighStartsAtPercent.HasValue || body.OvercrowdStartsAtPercent.HasValue)
        {
            if (m < 0 || m >= h || h >= o || o > 100)
                return BadRequest("Crowd thresholds must satisfy 0 ≤ medium < high < overcrowd ≤ 100.");
            b.CrowdMediumStartsAtPercent = m;
            b.CrowdHighStartsAtPercent = h;
            b.OvercrowdStartsAtPercent = o;
        }

        await _db.SaveChangesAsync(ct);
        await _queue.NotifyDashboardAsync(branchId, ct);
        return Ok(ToDetailDto(b));
    }

    private static BranchDetailDto ToDetailDto(Branch b) =>
        new(b.Id, b.Name, b.Location, b.MaxCapacity, b.CrowdMediumStartsAtPercent, b.CrowdHighStartsAtPercent, b.OvercrowdStartsAtPercent);
}

public record JoinQueueBody(
    [property: JsonPropertyName("serviceType")] BankServiceType ServiceType,
    [property: JsonPropertyName("isSimulated")] bool IsSimulated = false);

[ApiController]
[Route("api/branches/{branchId:int}/queue")]
[Authorize(Policy = "FloorStaff")]
[ServiceFilter(typeof(BranchRouteMatchesClaimFilter))]
public class QueueActionsController : ControllerBase
{
    private readonly IQueueOrchestrator _queue;

    public QueueActionsController(IQueueOrchestrator queue) => _queue = queue;

    [AllowAnonymous]
    [HttpPost("join")]
    public async Task<ActionResult<JoinQueueResultDto>> Join(int branchId, [FromBody] JoinQueueBody body, CancellationToken ct)
    {
        try
        {
            return Ok(await _queue.JoinQueueAsync(branchId, body.ServiceType, body.IsSimulated, ct));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("call-next")]
    public async Task<IActionResult> CallNext(int branchId, [FromBody] CallNextRequest body, CancellationToken ct)
    {
        try
        {
            await _queue.CallNextAsync(branchId, body.CounterId, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("skip")]
    public async Task<IActionResult> Skip(int branchId, [FromBody] TicketIdRequest body, CancellationToken ct)
    {
        try
        {
            await _queue.SkipTicketAsync(branchId, body.TicketId, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("complete")]
    public async Task<IActionResult> Complete(int branchId, [FromBody] TicketIdRequest body, CancellationToken ct)
    {
        try
        {
            await _queue.CompleteTicketAsync(branchId, body.TicketId, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("recall")]
    public async Task<IActionResult> Recall(int branchId, [FromBody] TicketIdRequest body, CancellationToken ct)
    {
        try
        {
            await _queue.RecallTicketAsync(branchId, body.TicketId, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("reset")]
    public async Task<IActionResult> Reset(int branchId, CancellationToken ct)
    {
        try
        {
            await _queue.ResetDailyAsync(branchId, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
