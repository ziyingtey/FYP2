using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QueueSystem.Api.Auth;
using QueueSystem.Api.Data;
using QueueSystem.Api.Models;
using QueueSystem.Api.Services;

namespace QueueSystem.Api.Controllers;

public record DailyReportResponse(
    [property: JsonPropertyName("branchId")] int BranchId,
    [property: JsonPropertyName("reportDate")] string ReportDate,
    [property: JsonPropertyName("totalCustomersServed")] int TotalCustomersServed,
    [property: JsonPropertyName("avgWaitMinutes")] double AvgWaitMinutes,
    [property: JsonPropertyName("peakHour")] string? PeakHour,
    [property: JsonPropertyName("busiestServiceCode")] string? BusiestServiceCode,
    [property: JsonPropertyName("generatedUtc")] DateTime GeneratedUtc
);

[ApiController]
[Route("api/branches/{branchId:int}/reports")]
[Authorize(Policy = "ManagerOnly")]
[ServiceFilter(typeof(BranchRouteMatchesClaimFilter))]
public class ReportsController : ControllerBase
{
    [HttpGet("daily")]
    public async Task<ActionResult<DailyReportResponse>> GetDaily(int branchId, [FromQuery] DateTime? date, AppDbContext db, CancellationToken ct)
    {
        var d = (date ?? DateTime.UtcNow).Date;
        var row = await db.DailyReports.AsNoTracking()
            .Include(r => r.BusiestService)
            .FirstOrDefaultAsync(r => r.BranchId == branchId && r.ReportDate == d, ct);
        if (row == null) return NotFound("No report for this date. POST daily/refresh to generate.");
        return Ok(Map(row));
    }

    [HttpPost("daily/refresh")]
    public async Task<ActionResult<DailyReportResponse>> RefreshDaily(int branchId, [FromQuery] DateTime? date, AppDbContext db, CancellationToken ct)
    {
        var d = (date ?? DateTime.UtcNow).Date;
        var row = await DailyReportAggregator.UpsertAsync(db, branchId, d, ct);
        await db.Entry(row).Reference(r => r.BusiestService).LoadAsync(ct);
        return Ok(Map(row));
    }

    private static DailyReportResponse Map(DailyReportAggregate r) =>
        new(
            r.BranchId,
            r.ReportDate.ToString("yyyy-MM-dd"),
            r.TotalCustomersServed,
            r.AvgWaitMinutes,
            r.PeakHour,
            r.BusiestService?.Code,
            r.GeneratedUtc);
}
