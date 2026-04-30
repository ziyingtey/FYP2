using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using QueueSystem.Api.Dtos;
using QueueSystem.Api.Models;
using QueueSystem.Api.Services;

namespace QueueSystem.Api.Controllers;

[ApiController]
[Route("api/branches/{branchId:int}")]
public class BranchesController : ControllerBase
{
    private readonly IQueueOrchestrator _queue;

    public BranchesController(IQueueOrchestrator queue) => _queue = queue;

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
}

public record JoinQueueBody([property: JsonPropertyName("serviceType")] BankServiceType ServiceType);

[ApiController]
[Route("api/branches/{branchId:int}/queue")]
public class QueueActionsController : ControllerBase
{
    private readonly IQueueOrchestrator _queue;

    public QueueActionsController(IQueueOrchestrator queue) => _queue = queue;

    [HttpPost("join")]
    public async Task<ActionResult<JoinQueueResultDto>> Join(int branchId, [FromBody] JoinQueueBody body, CancellationToken ct)
    {
        try
        {
            return Ok(await _queue.JoinQueueAsync(branchId, body.ServiceType, ct));
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
