using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QueueSystem.Api.Auth;
using QueueSystem.Api.Data;
using QueueSystem.Api.Dtos;
using QueueSystem.Api.Models;

namespace QueueSystem.Api.Controllers;

[ApiController]
[Route("api/branches/{branchId:int}/insights")]
[Authorize(Policy = "ManagerOnly")]
[ServiceFilter(typeof(BranchRouteMatchesClaimFilter))]
public class BranchInsightsController : ControllerBase
{
    private readonly AppDbContext _db;

    public BranchInsightsController(AppDbContext db) => _db = db;

    [HttpGet("crowd-logs")]
    public async Task<ActionResult<IReadOnlyList<CrowdLogDto>>> CrowdLogs(int branchId, [FromQuery] int take = 40, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 200);
        var rows = await _db.CrowdLogs.AsNoTracking()
            .Where(x => x.BranchId == branchId)
            .OrderByDescending(x => x.TimestampUtc)
            .Take(take)
            .Select(x => new CrowdLogDto(x.Id, x.TotalCustomers, x.CrowdLevel, x.TimestampUtc))
            .ToListAsync(ct);
        return Ok(rows);
    }

    [HttpGet("prediction-logs")]
    public async Task<ActionResult<IReadOnlyList<PredictionLogDto>>> PredictionLogs(int branchId, [FromQuery] int take = 60, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var rows = await _db.PredictionLogs.AsNoTracking()
            .Where(x => x.BranchId == branchId)
            .OrderByDescending(x => x.TimestampUtc)
            .Take(take)
            .Join(_db.BankServices.AsNoTracking(), p => p.ServiceId, s => s.Id, (p, s) => new PredictionLogDto(
                p.Id, s.Code, p.QueueLength, p.ActiveCounters, p.EstimatedAvgWaitMinutes, p.EstimatedClearingMinutes, p.Source, p.TimestampUtc))
            .ToListAsync(ct);
        return Ok(rows);
    }

    [HttpGet("simulation-runs")]
    public async Task<ActionResult<IReadOnlyList<SimulationRunDto>>> SimulationRuns(int branchId, [FromQuery] int take = 20, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 100);
        var runs = await _db.SimulationRuns.AsNoTracking()
            .Where(x => x.BranchId == branchId)
            .OrderByDescending(x => x.CreatedUtc)
            .Take(take)
            .ToListAsync(ct);
        var svcIds = runs.Where(r => r.FixedServiceId != null).Select(r => r.FixedServiceId!.Value).Distinct().ToList();
        var codes = await _db.BankServices.AsNoTracking()
            .Where(s => svcIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Code, ct);
        var dtos = runs.Select(r => new SimulationRunDto(
            r.Id,
            r.RequestedCount,
            r.GeneratedCount,
            r.Mode,
            r.Scenario,
            r.FixedServiceId.HasValue && codes.TryGetValue(r.FixedServiceId.Value, out var c) ? c : null,
            r.CreatedUtc)).ToList();
        return Ok(dtos);
    }

    [HttpGet("ticket-history")]
    public async Task<ActionResult<IReadOnlyList<TicketHistoryDto>>> TicketHistory(int branchId, [FromQuery] int take = 40, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 200);
        var tickets = await _db.QueueTickets.AsNoTracking()
            .Where(t => t.BranchId == branchId)
            .OrderByDescending(t => t.CreatedUtc)
            .Take(take)
            .ToListAsync(ct);
        var ids = tickets.Select(t => t.Id).ToList();
        var customers = await _db.Customers.AsNoTracking()
            .Where(c => ids.Contains(c.QueueTicketId))
            .ToDictionaryAsync(c => c.QueueTicketId, c => c.IsSimulated, ct);
        var services = await _db.BankServices.AsNoTracking().ToDictionaryAsync(s => s.Id, s => s.Code, ct);
        var list = tickets.Select(t => new TicketHistoryDto(
            t.Id,
            t.TicketCode,
            services.GetValueOrDefault(t.ServiceId, t.ServiceId.ToString()),
            t.Status.ToString(),
            t.CreatedUtc,
            t.CalledUtc,
            t.CompletedUtc,
            t.CounterId,
            customers.GetValueOrDefault(t.Id, false))).ToList();
        return Ok(list);
    }
}
