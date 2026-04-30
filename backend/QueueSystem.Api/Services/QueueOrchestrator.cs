using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QueueSystem.Api.Data;
using QueueSystem.Api.Dtos;
using QueueSystem.Api.Hubs;
using QueueSystem.Api.Models;

namespace QueueSystem.Api.Services;

public interface IQueueOrchestrator
{
    Task<BranchDashboardDto> GetDashboardAsync(int branchId, CancellationToken ct = default);
    Task<JoinQueueResultDto> JoinQueueAsync(int branchId, BankServiceType serviceType, CancellationToken ct = default);
    Task CallNextAsync(int branchId, int counterId, CancellationToken ct = default);
    Task SkipTicketAsync(int branchId, int ticketId, CancellationToken ct = default);
    Task CompleteTicketAsync(int branchId, int ticketId, CancellationToken ct = default);
    Task RecallTicketAsync(int branchId, int ticketId, CancellationToken ct = default);
    Task ResetDailyAsync(int branchId, CancellationToken ct = default);
    Task<int> SimulatorGenerateAsync(int branchId, int count, string mode, BankServiceType? fixedService, CancellationToken ct = default);
    Task SetCounterAsync(int counterId, bool? isOpen, bool? staffAvailable, BankServiceType? serviceType, CancellationToken ct = default);
}

public class QueueOrchestrator : IQueueOrchestrator
{
    private readonly AppDbContext _db;
    private readonly IHubContext<QueueHub> _hub;
    private readonly IMlPredictionClient _ml;
    private readonly ILogger<QueueOrchestrator> _logger;

    public QueueOrchestrator(
        AppDbContext db,
        IHubContext<QueueHub> hub,
        IMlPredictionClient ml,
        ILogger<QueueOrchestrator> logger)
    {
        _db = db;
        _hub = hub;
        _ml = ml;
        _logger = logger;
    }

    public async Task<BranchDashboardDto> GetDashboardAsync(int branchId, CancellationToken ct = default)
    {
        return await BuildDashboardAsync(branchId, ct);
    }

    public async Task<JoinQueueResultDto> JoinQueueAsync(int branchId, BankServiceType serviceType, CancellationToken ct = default)
    {
        var branch = await _db.Branches.AsNoTracking().FirstOrDefaultAsync(b => b.Id == branchId, ct);
        if (branch == null) throw new InvalidOperationException("Branch not found");

        var occ = await GetOccupancyAsync(branchId, ct);
        if (occ >= branch.MaxCapacity)
        {
            return new JoinQueueResultDto("", serviceType.ToString(), 0, true, "Branch at maximum capacity. Queue booking is temporarily disabled.");
        }

        var code = await NextTicketCodeAsync(branchId, serviceType, ct);
        var ticket = new QueueTicket
        {
            BranchId = branchId,
            ServiceType = serviceType,
            TicketCode = code,
            Status = TicketStatus.Waiting,
            CreatedUtc = DateTime.UtcNow
        };
        _db.QueueTickets.Add(ticket);
        await _db.SaveChangesAsync(ct);

        var waitingSame = await _db.QueueTickets.CountAsync(
            t => t.BranchId == branchId && t.ServiceType == serviceType && t.Status == TicketStatus.Waiting, ct);

        await BroadcastAsync(branchId, ct);
        return new JoinQueueResultDto(code, serviceType.ToString(), waitingSame, false, null);
    }

    public async Task CallNextAsync(int branchId, int counterId, CancellationToken ct = default)
    {
        var counter = await _db.Counters.FirstOrDefaultAsync(c => c.Id == counterId && c.BranchId == branchId, ct);
        if (counter == null) throw new InvalidOperationException("Counter not found");
        if (!counter.IsOpen || !counter.StaffAvailable) throw new InvalidOperationException("Counter is not available");

        var existingServing = await _db.QueueTickets
            .Where(t => t.BranchId == branchId && t.CounterId == counterId && t.Status == TicketStatus.Serving)
            .ToListAsync(ct);
        foreach (var t in existingServing)
        {
            t.Status = TicketStatus.Skipped;
            t.CompletedUtc = DateTime.UtcNow;
        }

        var next = await _db.QueueTickets
            .Where(t => t.BranchId == branchId && t.ServiceType == counter.ServiceType && t.Status == TicketStatus.Waiting)
            .OrderBy(t => t.RecalledUtc == null ? 1 : 0)
            .ThenBy(t => t.CreatedUtc)
            .FirstOrDefaultAsync(ct);
        if (next == null) { await _db.SaveChangesAsync(ct); await BroadcastAsync(branchId, ct); return; }

        next.Status = TicketStatus.Serving;
        next.CounterId = counterId;
        next.CalledUtc = DateTime.UtcNow;
        next.RecalledUtc = null;
        await _db.SaveChangesAsync(ct);
        await BroadcastAsync(branchId, ct);
    }

    public async Task SkipTicketAsync(int branchId, int ticketId, CancellationToken ct = default)
    {
        var ticket = await _db.QueueTickets.FirstOrDefaultAsync(t => t.Id == ticketId && t.BranchId == branchId, ct);
        if (ticket == null) throw new InvalidOperationException("Ticket not found");
        if (ticket.Status != TicketStatus.Serving) throw new InvalidOperationException("Only the currently serving ticket can be skipped");
        ticket.Status = TicketStatus.Skipped;
        ticket.CompletedUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        await BroadcastAsync(branchId, ct);
    }

    public async Task CompleteTicketAsync(int branchId, int ticketId, CancellationToken ct = default)
    {
        var ticket = await _db.QueueTickets.FirstOrDefaultAsync(t => t.Id == ticketId && t.BranchId == branchId, ct);
        if (ticket == null) throw new InvalidOperationException("Ticket not found");
        if (ticket.Status != TicketStatus.Serving) throw new InvalidOperationException("Only the serving ticket can be completed");
        ticket.Status = TicketStatus.Completed;
        ticket.CompletedUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        await BroadcastAsync(branchId, ct);
    }

    public async Task RecallTicketAsync(int branchId, int ticketId, CancellationToken ct = default)
    {
        var ticket = await _db.QueueTickets.FirstOrDefaultAsync(t => t.Id == ticketId && t.BranchId == branchId, ct);
        if (ticket == null) throw new InvalidOperationException("Ticket not found");
        if (ticket.Status != TicketStatus.Serving) throw new InvalidOperationException("Recall applies to the ticket currently being served");
        ticket.Status = TicketStatus.Waiting;
        ticket.RecalledUtc = DateTime.UtcNow;
        ticket.CounterId = null;
        ticket.CalledUtc = null;
        await _db.SaveChangesAsync(ct);
        await BroadcastAsync(branchId, ct);
    }

    public async Task ResetDailyAsync(int branchId, CancellationToken ct = default)
    {
        var open = await _db.QueueTickets.Where(t => t.BranchId == branchId && t.Status != TicketStatus.Completed).ToListAsync(ct);
        foreach (var t in open)
        {
            t.Status = TicketStatus.Skipped;
            t.CompletedUtc = DateTime.UtcNow;
        }

        var today = DateTime.UtcNow.Date;
        var seq = await _db.QueueDaySequences.Where(q => q.BranchId == branchId && q.DateUtc == today).ToListAsync(ct);
        _db.QueueDaySequences.RemoveRange(seq);
        await _db.SaveChangesAsync(ct);
        await BroadcastAsync(branchId, ct);
    }

    public async Task<int> SimulatorGenerateAsync(int branchId, int count, string mode, BankServiceType? fixedService, CancellationToken ct = default)
    {
        var rnd = new Random();
        var added = 0;
        for (var i = 0; i < count; i++)
        {
            BankServiceType st;
            if (string.Equals(mode, "manual", StringComparison.OrdinalIgnoreCase) && fixedService.HasValue)
                st = fixedService.Value;
            else
                st = (BankServiceType)rnd.Next(0, 3);

            var result = await JoinQueueAsync(branchId, st, ct);
            if (!result.QueueBookingBlocked) added++;
            else break;
        }
        return added;
    }

    public async Task SetCounterAsync(int counterId, bool? isOpen, bool? staffAvailable, BankServiceType? serviceType, CancellationToken ct = default)
    {
        var c = await _db.Counters.FirstOrDefaultAsync(x => x.Id == counterId, ct)
                ?? throw new InvalidOperationException("Counter not found");
        if (isOpen.HasValue) c.IsOpen = isOpen.Value;
        if (staffAvailable.HasValue) c.StaffAvailable = staffAvailable.Value;
        if (serviceType.HasValue) c.ServiceType = serviceType.Value;
        await _db.SaveChangesAsync(ct);
        await BroadcastAsync(c.BranchId, ct);
    }

    private async Task<int> GetOccupancyAsync(int branchId, CancellationToken ct) =>
        await _db.QueueTickets.CountAsync(
            t => t.BranchId == branchId && (t.Status == TicketStatus.Waiting || t.Status == TicketStatus.Serving), ct);

    private async Task<string> NextTicketCodeAsync(int branchId, BankServiceType serviceType, CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var seq = await _db.QueueDaySequences.FirstOrDefaultAsync(
            s => s.BranchId == branchId && s.ServiceType == serviceType && s.DateUtc == today, ct);
        if (seq == null)
        {
            seq = new QueueDaySequence { BranchId = branchId, ServiceType = serviceType, DateUtc = today, LastNumber = 0 };
            _db.QueueDaySequences.Add(seq);
            await _db.SaveChangesAsync(ct);
        }

        seq.LastNumber++;
        await _db.SaveChangesAsync(ct);
        var prefix = serviceType switch
        {
            BankServiceType.General => "G",
            BankServiceType.Card => "C",
            BankServiceType.Wealth => "W",
            _ => "Q"
        };
        return $"{prefix}{seq.LastNumber:000}";
    }

    private async Task<double> AvgServiceMinutesAsync(int branchId, BankServiceType serviceType, CancellationToken ct)
    {
        var completed = await _db.QueueTickets
            .AsNoTracking()
            .Where(t => t.BranchId == branchId && t.ServiceType == serviceType && t.Status == TicketStatus.Completed
                        && t.CalledUtc != null && t.CompletedUtc != null)
            .OrderByDescending(t => t.CompletedUtc)
            .Take(50)
            .Select(t => new { a = t.CalledUtc!.Value, b = t.CompletedUtc!.Value })
            .ToListAsync(ct);
        if (completed.Count == 0) return 5.0;
        return completed.Average(x => (x.b - x.a).TotalMinutes);
    }

    private async Task<BranchDashboardDto> BuildDashboardAsync(int branchId, CancellationToken ct)
    {
        var branch = await _db.Branches.AsNoTracking().FirstOrDefaultAsync(b => b.Id == branchId, ct)
                     ?? throw new InvalidOperationException("Branch not found");

        var occ = await GetOccupancyAsync(branchId, ct);
        var pct = branch.MaxCapacity <= 0 ? 0 : occ * 100.0 / branch.MaxCapacity;
        var crowd = CrowdMetrics.Level(pct);
        var blocked = occ >= branch.MaxCapacity;

        var counters = await _db.Counters.AsNoTracking().Where(c => c.BranchId == branchId).OrderBy(c => c.Id).ToListAsync(ct);
        var tickets = await _db.QueueTickets.AsNoTracking().Where(t => t.BranchId == branchId).ToListAsync(ct);

        var counterDtos = new List<CounterStateDto>();
        foreach (var c in counters)
        {
            var serving = tickets.FirstOrDefault(t => t.CounterId == c.Id && t.Status == TicketStatus.Serving);
            counterDtos.Add(new CounterStateDto(c.Id, c.Label, c.ServiceType.ToString(), c.IsOpen, c.StaffAvailable,
                serving?.TicketCode, serving?.Id));
        }

        var services = new List<ServiceQueueStateDto>();
        foreach (BankServiceType st in Enum.GetValues<BankServiceType>())
        {
            var waiting = tickets.Count(t => t.ServiceType == st && t.Status == TicketStatus.Waiting);
            var serving = tickets.FirstOrDefault(t => t.ServiceType == st && t.Status == TicketStatus.Serving);
            var activeCounters = counters.Count(x => x.ServiceType == st && x.IsOpen && x.StaffAvailable);
            var avg = await AvgServiceMinutesAsync(branchId, st, ct);
            var mlReq = new MlPredictionRequest(waiting, activeCounters, st.ToString(), avg);
            var ml = await _ml.PredictAsync(mlReq, ct);
            var display = st switch
            {
                BankServiceType.General => "General Banking",
                BankServiceType.Card => "Card Services",
                BankServiceType.Wealth => "Wealth",
                _ => st.ToString()
            };
            services.Add(new ServiceQueueStateDto(st.ToString(), display, waiting, serving?.TicketCode, serving?.Id, serving?.CounterId,
                ml.EstimatedClearingMinutes, ml.EstimatedAvgWaitMinutes));
        }

        var recent = tickets
            .OrderByDescending(t => t.CreatedUtc)
            .Take(15)
            .Select(t => new TicketSummaryDto(t.TicketCode, t.ServiceType.ToString(), t.Status.ToString(), t.CreatedUtc))
            .ToList();

        return new BranchDashboardDto(
            branch.Id,
            branch.Name,
            branch.MaxCapacity,
            occ,
            Math.Round(pct, 1),
            crowd,
            blocked,
            blocked ? "Maximum branch capacity reached." : null,
            services,
            counterDtos,
            recent);
    }

    private async Task BroadcastAsync(int branchId, CancellationToken ct)
    {
        var dto = await BuildDashboardAsync(branchId, ct);
        await _hub.Clients.Group(QueueHub.BranchGroup(branchId)).SendAsync("dashboard", dto, ct);
    }
}
