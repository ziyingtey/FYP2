using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QueueSystem.Api.Auth;
using QueueSystem.Api.Data;
using QueueSystem.Api.Dtos;
using QueueSystem.Api.Models;

namespace QueueSystem.Api.Controllers;

[ApiController]
[Route("api/branches/{branchId:int}/staff")]
[Authorize(Policy = "FloorStaff")]
[ServiceFilter(typeof(BranchRouteMatchesClaimFilter))]
public class BranchStaffController : ControllerBase
{
    private readonly AppDbContext _db;

    public BranchStaffController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StaffListItemDto>>> List(int branchId, CancellationToken ct)
    {
        var staff = await _db.StaffMembers.AsNoTracking()
            .Where(s => s.BranchId == branchId)
            .OrderBy(s => s.Id)
            .ToListAsync(ct);
        var counters = await _db.Counters.AsNoTracking()
            .Where(c => c.BranchId == branchId && c.StaffId != null)
            .ToListAsync(ct);
        var byStaff = counters.ToDictionary(c => c.StaffId!.Value, c => c.Label);
        var list = staff.Select(s => new StaffListItemDto(
            s.Id,
            s.Name,
            s.Role == StaffRole.Manager ? "Manager" : "Staff",
            s.Email,
            byStaff.TryGetValue(s.Id, out var lbl) ? lbl : null)).ToList();
        return Ok(list);
    }
}
