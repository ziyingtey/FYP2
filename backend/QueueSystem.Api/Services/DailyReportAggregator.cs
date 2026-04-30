using Microsoft.EntityFrameworkCore;
using QueueSystem.Api.Data;
using QueueSystem.Api.Models;

namespace QueueSystem.Api.Services;

public static class DailyReportAggregator
{
    /// <summary>Computes aggregates for the given UTC calendar day and upserts <see cref="DailyReportAggregate"/>.</summary>
    public static async Task<DailyReportAggregate> UpsertAsync(AppDbContext db, int branchId, DateTime reportDateUtc, CancellationToken ct = default)
    {
        if (!await db.Branches.AnyAsync(b => b.Id == branchId, ct))
            throw new InvalidOperationException("Branch not found");

        var start = reportDateUtc.Date;
        var end = start.AddDays(1);

        var completed = await db.QueueTickets
            .AsNoTracking()
            .Where(t => t.BranchId == branchId && t.Status == TicketStatus.Completed && t.CompletedUtc >= start && t.CompletedUtc < end)
            .ToListAsync(ct);

        var dayTickets = await db.QueueTickets
            .AsNoTracking()
            .Where(t => t.BranchId == branchId && t.CreatedUtc >= start && t.CreatedUtc < end)
            .ToListAsync(ct);

        var waits = completed
            .Where(t => t.CalledUtc != null && t.CreatedUtc != default)
            .Select(t => (t.CalledUtc!.Value - t.CreatedUtc).TotalMinutes)
            .ToList();

        var avgWait = waits.Count == 0 ? 0 : Math.Round(waits.Average(), 1);

        var peakHour = ComputePeakHourLabel(dayTickets);
        var busiestServiceId = completed.GroupBy(t => t.ServiceId).OrderByDescending(g => g.Count()).Select(g => (int?)g.Key).FirstOrDefault();

        var existing = await db.DailyReports.FirstOrDefaultAsync(r => r.BranchId == branchId && r.ReportDate == start, ct);
        if (existing != null)
        {
            existing.TotalCustomersServed = completed.Count;
            existing.AvgWaitMinutes = avgWait;
            existing.PeakHour = peakHour;
            existing.BusiestServiceId = busiestServiceId;
            existing.GeneratedUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            return existing;
        }

        var row = new DailyReportAggregate
        {
            BranchId = branchId,
            ReportDate = start,
            TotalCustomersServed = completed.Count,
            AvgWaitMinutes = avgWait,
            PeakHour = peakHour,
            BusiestServiceId = busiestServiceId,
            GeneratedUtc = DateTime.UtcNow
        };
        db.DailyReports.Add(row);
        await db.SaveChangesAsync(ct);
        return row;
    }

    private static string? ComputePeakHourLabel(IReadOnlyList<QueueTicket> dayTickets)
    {
        if (dayTickets.Count == 0) return null;
        var g = dayTickets.GroupBy(t => t.CreatedUtc.Hour).OrderByDescending(x => x.Count()).First();
        var h = g.Key;
        var next = (h + 1) % 24;
        return $"{h:00}:00–{next:00}:00 UTC";
    }
}
