using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QueueSystem.Api.Data;
using QueueSystem.Api.Dtos;
using QueueSystem.Api.Models;
using QueueSystem.Api.Services;

namespace QueueSystem.Api.Controllers;

/// <summary>Public branch list with live occupancy for multi-window demos (compare branches).</summary>
[ApiController]
[Route("api")]
public class BranchDirectoryController : ControllerBase
{
    private readonly AppDbContext _db;

    public BranchDirectoryController(AppDbContext db) => _db = db;

    [HttpGet("branches")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<BranchSummaryDto>>> ListBranches(CancellationToken ct)
    {
        var branches = await _db.Branches.AsNoTracking().OrderBy(b => b.Id).ToListAsync(ct);
        var occRows = await _db.QueueTickets.AsNoTracking()
            .Where(t => t.Status == TicketStatus.Waiting || t.Status == TicketStatus.Serving)
            .GroupBy(t => t.BranchId)
            .Select(g => new { BranchId = g.Key, Cnt = g.Count() })
            .ToListAsync(ct);
        var occMap = occRows.ToDictionary(x => x.BranchId, x => x.Cnt);

        var list = new List<BranchSummaryDto>(branches.Count);
        foreach (var b in branches)
        {
            var occ = occMap.GetValueOrDefault(b.Id, 0);
            var pct = b.MaxCapacity <= 0 ? 0 : occ * 100.0 / b.MaxCapacity;
            var crowd = CrowdMetrics.Level(pct, b.CrowdMediumStartsAtPercent, b.CrowdHighStartsAtPercent, b.OvercrowdStartsAtPercent);
            var blocked = occ >= b.MaxCapacity;
            list.Add(new BranchSummaryDto(
                b.Id,
                b.Name,
                b.Location,
                b.MaxCapacity,
                occ,
                Math.Round(pct, 1),
                crowd,
                blocked));
        }

        return Ok(list);
    }
}
