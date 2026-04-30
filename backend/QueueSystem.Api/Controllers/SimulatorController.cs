using Microsoft.AspNetCore.Mvc;
using QueueSystem.Api.Dtos;
using QueueSystem.Api.Models;
using QueueSystem.Api.Services;

namespace QueueSystem.Api.Controllers;

[ApiController]
[Route("api/branches/{branchId:int}/simulator")]
public class SimulatorController : ControllerBase
{
    private readonly IQueueOrchestrator _queue;

    public SimulatorController(IQueueOrchestrator queue) => _queue = queue;

    [HttpPost("generate")]
    public async Task<ActionResult<object>> Generate(int branchId, [FromBody] SimulatorGenerateRequest body, CancellationToken ct)
    {
        var n = await _queue.SimulatorGenerateAsync(branchId, body.Count, body.Mode, body.ServiceType, ct);
        return Ok(new { generated = n, message = n < body.Count ? "Stopped early: capacity limit reached." : "OK" });
    }
}

[ApiController]
[Route("api/counters")]
public class CountersController : ControllerBase
{
    private readonly IQueueOrchestrator _queue;

    public CountersController(IQueueOrchestrator queue) => _queue = queue;

    [HttpPatch("{counterId:int}")]
    public async Task<IActionResult> Patch(int counterId, [FromBody] PatchCounterRequest body, CancellationToken ct)
    {
        try
        {
            await _queue.SetCounterAsync(counterId, body.IsOpen, body.StaffAvailable, body.ServiceType, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
