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
    Task<JoinQueueResultDto> JoinQueueAsync(int branchId, BankServiceType serviceType, bool isSimulated = false, CancellationToken ct = default);
    Task CallNextAsync(int branchId, int counterId, CancellationToken ct = default);
    Task SkipTicketAsync(int branchId, int ticketId, CancellationToken ct = default);
    Task CompleteTicketAsync(int branchId, int ticketId, CancellationToken ct = default);
    Task RecallTicketAsync(int branchId, int ticketId, CancellationToken ct = default);
    Task ResetDailyAsync(int branchId, CancellationToken ct = default);
    Task<int> SimulatorGenerateAsync(int branchId, int count, string mode, BankServiceType? fixedService, string? scenario, CancellationToken ct = default);
    Task SetCounterAsync(int counterId, bool? isOpen, bool? staffAvailable, BankServiceType? serviceType, CancellationToken ct = default);
    Task NotifyDashboardAsync(int branchId, CancellationToken ct = default);
}

public class QueueOrchestrator : IQueueOrchestrator
{
    private readonly AppDbContext _db;
    private readonly IHubContext<QueueHub> _hub;
    private readonly IMlPredictionClient _ml;

    public QueueOrchestrator(
        AppDbContext db,
        IHubContext<QueueHub> hub,
        IMlPredictionClient ml)
    {
        _db = db;
        _hub = hub;
        _ml = ml;
    }

    public async Task<BranchDashboardDto> GetDashboardAsync(int branchId, CancellationToken ct = default)
    {
        var (dto, _, _) = await BuildDashboardSnapshotAsync(branchId, captureLogs: false, ct);
        return dto;
    }

    public async Task<JoinQueueResultDto> JoinQueueAsync(int branchId, BankServiceType serviceType, bool isSimulated = false, CancellationToken ct = default)
    {
        var serviceId = ServiceIdMapping.FromApi(serviceType);
        var branch = await _db.Branches.AsNoTracking().FirstOrDefaultAsync(b => b.Id == branchId, ct);
        if (branch == null) throw new InvalidOperationException("Branch not found");

        var occ = await GetOccupancyAsync(branchId, ct);
        if (occ >= branch.MaxCapacity)
        {
            return new JoinQueueResultDto("", serviceType.ToString(), 0, true, "Branch at maximum capacity. Queue booking is temporarily disabled.");
        }

        var code = await NextTicketCodeAsync(branchId, serviceId, ct);
        var ticket = new QueueTicket
        {
            BranchId = branchId,
            ServiceId = serviceId,
            TicketCode = code,
            Status = TicketStatus.Waiting,
            CreatedUtc = DateTime.UtcNow
        };
        _db.QueueTickets.Add(ticket);
        await _db.SaveChangesAsync(ct);

        _db.Customers.Add(new Customer
        {
            QueueTicketId = ticket.Id,
            ArrivalUtc = DateTime.UtcNow,
            IsSimulated = isSimulated
        });
        await _db.SaveChangesAsync(ct);

        var waitingSame = await _db.QueueTickets.CountAsync(
            t => t.BranchId == branchId && t.ServiceId == serviceId && t.Status == TicketStatus.Waiting, ct);

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
            await ReleaseOpenAssignmentsAsync(t.Id, ct);
            t.Status = TicketStatus.Skipped;
            t.CompletedUtc = DateTime.UtcNow;
        }

        var next = await _db.QueueTickets
            .Where(t => t.BranchId == branchId && t.ServiceId == counter.ServiceId && t.Status == TicketStatus.Waiting)
            .OrderBy(t => t.RecalledUtc == null ? 1 : 0)
            .ThenBy(t => t.CreatedUtc)
            .FirstOrDefaultAsync(ct);
        if (next == null)
        {
            await _db.SaveChangesAsync(ct);
            await BroadcastAsync(branchId, ct);
            return;
        }

        next.Status = TicketStatus.Serving;
        next.CounterId = counterId;
        next.CalledUtc = DateTime.UtcNow;
        next.RecalledUtc = null;
        _db.QueueAssignments.Add(new QueueAssignment
        {
            QueueTicketId = next.Id,
            CounterId = counterId,
            AssignedUtc = DateTime.UtcNow,
            Kind = QueueAssignmentKind.Call
        });
        await _db.SaveChangesAsync(ct);
        await BroadcastAsync(branchId, ct);
    }

    public async Task SkipTicketAsync(int branchId, int ticketId, CancellationToken ct = default)
    {
        var ticket = await _db.QueueTickets.FirstOrDefaultAsync(t => t.Id == ticketId && t.BranchId == branchId, ct);
        if (ticket == null) throw new InvalidOperationException("Ticket not found");
        if (ticket.Status != TicketStatus.Serving) throw new InvalidOperationException("Only the currently serving ticket can be skipped");
        await ReleaseOpenAssignmentsAsync(ticket.Id, ct);
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
        await ReleaseOpenAssignmentsAsync(ticket.Id, ct);
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
        await ReleaseOpenAssignmentsAsync(ticket.Id, ct);
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
            await ReleaseOpenAssignmentsAsync(t.Id, ct);
            t.Status = TicketStatus.Skipped;
            t.CompletedUtc = DateTime.UtcNow;
        }

        var today = DateTime.UtcNow.Date;
        var seq = await _db.QueueDaySequences.Where(q => q.BranchId == branchId && q.DateUtc == today).ToListAsync(ct);
        _db.QueueDaySequences.RemoveRange(seq);
        await _db.SaveChangesAsync(ct);
        await BroadcastAsync(branchId, ct);
    }

    public async Task<int> SimulatorGenerateAsync(int branchId, int count, string mode, BankServiceType? fixedService, string? scenario = null, CancellationToken ct = default)
    {
        var effectiveCount = SimulationScenarioHelper.AdjustCount(count, scenario);
        var rnd = new Random();
        var added = 0;
        for (var i = 0; i < effectiveCount; i++)
        {
            BankServiceType st;
            if (string.Equals(mode, "manual", StringComparison.OrdinalIgnoreCase) && fixedService.HasValue)
                st = fixedService.Value;
            else
                st = SimulationScenarioHelper.PickRandomService(rnd, scenario);

            var result = await JoinQueueAsync(branchId, st, isSimulated: true, ct);
            if (!result.QueueBookingBlocked) added++;
            else break;
        }

        _db.SimulationRuns.Add(new SimulationRun
        {
            BranchId = branchId,
            StartedByStaffId = null,
            RequestedCount = effectiveCount,
            GeneratedCount = added,
            Mode = mode,
            Scenario = string.IsNullOrWhiteSpace(scenario) ? null : scenario.Trim(),
            FixedServiceId = fixedService.HasValue ? ServiceIdMapping.FromApi(fixedService.Value) : null,
            CreatedUtc = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);
        return added;
    }

    public async Task SetCounterAsync(int counterId, bool? isOpen, bool? staffAvailable, BankServiceType? serviceType, CancellationToken ct = default)
    {
        var c = await _db.Counters.FirstOrDefaultAsync(x => x.Id == counterId, ct)
                ?? throw new InvalidOperationException("Counter not found");
        if (isOpen.HasValue) c.IsOpen = isOpen.Value;
        if (staffAvailable.HasValue) c.StaffAvailable = staffAvailable.Value;
        if (serviceType.HasValue) c.ServiceId = ServiceIdMapping.FromApi(serviceType.Value);
        await _db.SaveChangesAsync(ct);
        await BroadcastAsync(c.BranchId, ct);
    }

    public async Task NotifyDashboardAsync(int branchId, CancellationToken ct = default)
    {
        var (dto, _, _) = await BuildDashboardSnapshotAsync(branchId, captureLogs: false, ct);
        await _hub.Clients.Group(QueueHub.BranchGroup(branchId)).SendAsync("dashboard", dto, ct);
    }

    private async Task ReleaseOpenAssignmentsAsync(int ticketId, CancellationToken ct)
    {
        var open = await _db.QueueAssignments.Where(a => a.QueueTicketId == ticketId && a.ReleasedUtc == null).ToListAsync(ct);
        var now = DateTime.UtcNow;
        foreach (var a in open) a.ReleasedUtc = now;
    }

    private async Task<int> GetOccupancyAsync(int branchId, CancellationToken ct) =>
        await _db.QueueTickets.CountAsync(
            t => t.BranchId == branchId && (t.Status == TicketStatus.Waiting || t.Status == TicketStatus.Serving), ct);

    private async Task<string> NextTicketCodeAsync(int branchId, int serviceId, CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var seq = await _db.QueueDaySequences.FirstOrDefaultAsync(
            s => s.BranchId == branchId && s.ServiceId == serviceId && s.DateUtc == today, ct);
        if (seq == null)
        {
            seq = new QueueDaySequence { BranchId = branchId, ServiceId = serviceId, DateUtc = today, LastNumber = 0 };
            _db.QueueDaySequences.Add(seq);
            await _db.SaveChangesAsync(ct);
        }

        seq.LastNumber++;
        await _db.SaveChangesAsync(ct);
        var prefix = ServiceIdMapping.TicketPrefix(serviceId);
        return $"{prefix}{seq.LastNumber:000}";
    }

    private async Task<double> AvgServiceMinutesAsync(int branchId, int serviceId, CancellationToken ct)
    {
        var completed = await _db.QueueTickets
            .AsNoTracking()
            .Where(t => t.BranchId == branchId && t.ServiceId == serviceId && t.Status == TicketStatus.Completed
                        && t.CalledUtc != null && t.CompletedUtc != null)
            .OrderByDescending(t => t.CompletedUtc)
            .Take(50)
            .Select(t => new { a = t.CalledUtc!.Value, b = t.CompletedUtc!.Value })
            .ToListAsync(ct);
        if (completed.Count == 0) return 5.0;
        return completed.Average(x => (x.b - x.a).TotalMinutes);
    }

    private async Task<(BranchDashboardDto Dto, CrowdLog? Crowd, List<PredictionLog> Predictions)> BuildDashboardSnapshotAsync(
        int branchId, bool captureLogs, CancellationToken ct)
    {
        var branch = await _db.Branches.AsNoTracking().FirstOrDefaultAsync(b => b.Id == branchId, ct)
                     ?? throw new InvalidOperationException("Branch not found");

        var occ = await GetOccupancyAsync(branchId, ct);
        var pct = branch.MaxCapacity <= 0 ? 0 : occ * 100.0 / branch.MaxCapacity;
        var crowd = CrowdMetrics.Level(pct, branch.CrowdMediumStartsAtPercent, branch.CrowdHighStartsAtPercent, branch.OvercrowdStartsAtPercent);
        var blocked = occ >= branch.MaxCapacity;

        var counters = await _db.Counters.AsNoTracking().Where(c => c.BranchId == branchId).OrderBy(c => c.Id).ToListAsync(ct);
        var tickets = await _db.QueueTickets.AsNoTracking().Where(t => t.BranchId == branchId).ToListAsync(ct);
        var servicesCatalog = await _db.BankServices.AsNoTracking().OrderBy(s => s.Id).ToListAsync(ct);

        var counterDtos = new List<CounterStateDto>();
        foreach (var c in counters)
        {
            var serving = tickets.FirstOrDefault(t => t.CounterId == c.Id && t.Status == TicketStatus.Serving);
            var svc = servicesCatalog.FirstOrDefault(s => s.Id == c.ServiceId);
            counterDtos.Add(new CounterStateDto(c.Id, c.Label, svc?.Code ?? c.ServiceId.ToString(), c.IsOpen, c.StaffAvailable,
                serving?.TicketCode, serving?.Id));
        }

        var serviceDtos = new List<ServiceQueueStateDto>();
        var predRows = new List<PredictionLog>();
        foreach (var svc in servicesCatalog)
        {
            var waiting = tickets.Count(t => t.ServiceId == svc.Id && t.Status == TicketStatus.Waiting);
            var serving = tickets.FirstOrDefault(t => t.ServiceId == svc.Id && t.Status == TicketStatus.Serving);
            var activeCounters = counters.Count(x => x.ServiceId == svc.Id && x.IsOpen && x.StaffAvailable);
            var avg = await AvgServiceMinutesAsync(branchId, svc.Id, ct);
            var avgInput = avg > 0 ? avg : svc.AvgServiceTimeMinutes;
            var mlReq = new MlPredictionRequest(waiting, activeCounters, svc.Code, avgInput);
            var ml = await _ml.PredictAsync(mlReq, ct);
            var display = svc.DisplayName;
            serviceDtos.Add(new ServiceQueueStateDto(svc.Code, display, waiting, serving?.TicketCode, serving?.Id, serving?.CounterId,
                ml.EstimatedClearingMinutes, ml.EstimatedAvgWaitMinutes));

            if (captureLogs)
            {
                predRows.Add(new PredictionLog
                {
                    BranchId = branchId,
                    ServiceId = svc.Id,
                    EstimatedAvgWaitMinutes = ml.EstimatedAvgWaitMinutes,
                    EstimatedClearingMinutes = ml.EstimatedClearingMinutes,
                    QueueLength = waiting,
                    ActiveCounters = activeCounters,
                    AvgServiceMinutesInput = avgInput,
                    Source = "ML",
                    TimestampUtc = DateTime.UtcNow
                });
            }
        }

        var recent = tickets
            .OrderByDescending(t => t.CreatedUtc)
            .Take(15)
            .Select(t =>
            {
                var svc = servicesCatalog.FirstOrDefault(s => s.Id == t.ServiceId);
                return new TicketSummaryDto(t.TicketCode, svc?.Code ?? t.ServiceId.ToString(), t.Status.ToString(), t.CreatedUtc);
            })
            .ToList();

        var dto = new BranchDashboardDto(
            branch.Id,
            branch.Name,
            branch.Location,
            branch.MaxCapacity,
            occ,
            Math.Round(pct, 1),
            crowd,
            blocked,
            blocked ? "Maximum branch capacity reached." : null,
            branch.CrowdMediumStartsAtPercent,
            branch.CrowdHighStartsAtPercent,
            branch.OvercrowdStartsAtPercent,
            serviceDtos,
            counterDtos,
            recent);

        CrowdLog? crowdRow = null;
        if (captureLogs)
        {
            crowdRow = new CrowdLog
            {
                BranchId = branchId,
                TotalCustomers = occ,
                CrowdLevel = crowd,
                TimestampUtc = DateTime.UtcNow
            };
        }

        return (dto, crowdRow, predRows);
    }

    private async Task BroadcastAsync(int branchId, CancellationToken ct)
    {
        var (dto, crowd, preds) = await BuildDashboardSnapshotAsync(branchId, captureLogs: true, ct);
        if (crowd != null) _db.CrowdLogs.Add(crowd);
        if (preds.Count > 0) _db.PredictionLogs.AddRange(preds);
        if (crowd != null || preds.Count > 0) await _db.SaveChangesAsync(ct);

        await _hub.Clients.Group(QueueHub.BranchGroup(branchId)).SendAsync("dashboard", dto, ct);
    }
}
