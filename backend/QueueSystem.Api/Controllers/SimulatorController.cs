using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QueueSystem.Api.Auth;
using QueueSystem.Api.Dtos;
using QueueSystem.Api.Models;
using QueueSystem.Api.Services;
using QueueSystem.Api.Data;

namespace QueueSystem.Api.Controllers;

[ApiController]
[Route("api/branches/{branchId:int}/simulator")]
[Authorize(Policy = "ManagerOnly")]
[ServiceFilter(typeof(BranchRouteMatchesClaimFilter))]
public class SimulatorController : ControllerBase
{
    private readonly IQueueOrchestrator _queue;

    public SimulatorController(IQueueOrchestrator queue) => _queue = queue;

    [HttpPost("generate")]
    public async Task<ActionResult<object>> Generate(int branchId, [FromBody] SimulatorGenerateRequest body, CancellationToken ct)
    {
        var requestedEffective = SimulationScenarioHelper.AdjustCount(body.Count, body.Scenario);
        var n = await _queue.SimulatorGenerateAsync(branchId, body.Count, body.Mode, body.ServiceType, body.Scenario, ct);
        var message = n < requestedEffective
            ? "Stopped early: branch at capacity (join attempts blocked)."
            : $"Issued {n} ticket(s) from {requestedEffective} simulated join attempt(s).";
        return Ok(new
        {
            generated = n,
            requested = requestedEffective,
            scenario = body.Scenario,
            message
        });
    }
}

[ApiController]
[Route("api/counters")]
[Authorize(Policy = "FloorStaff")]
public class CountersController : ControllerBase
{
    private readonly IQueueOrchestrator _queue;
    private readonly AppDbContext _db;

    public CountersController(IQueueOrchestrator queue, AppDbContext db)
    {
        _queue = queue;
        _db = db;
    }

    [HttpPatch("{counterId:int}")]
    public async Task<IActionResult> Patch(int counterId, [FromBody] PatchCounterRequest body, CancellationToken ct)
    {
        try
        {
            var counter = await _db.Counters.AsNoTracking().FirstOrDefaultAsync(c => c.Id == counterId, ct);
            if (counter == null) return NotFound();
            var claim = User.FindFirst(AuthRoles.BranchIdClaim)?.Value;
            if (claim is null || counter.BranchId.ToString() != claim)
                return Forbid();

            await _queue.SetCounterAsync(counterId, body.IsOpen, body.StaffAvailable, body.ServiceType, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
