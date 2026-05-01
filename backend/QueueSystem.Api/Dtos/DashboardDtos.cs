using System.Text.Json.Serialization;
using QueueSystem.Api.Models;

namespace QueueSystem.Api.Dtos;

public record ServiceQueueStateDto(
    [property: JsonPropertyName("serviceType")] string ServiceType,
    [property: JsonPropertyName("displayName")] string DisplayName,
    [property: JsonPropertyName("waiting")] int Waiting,
    [property: JsonPropertyName("servingTicketCode")] string? ServingTicketCode,
    [property: JsonPropertyName("servingTicketId")] int? ServingTicketId,
    [property: JsonPropertyName("servingCounterId")] int? ServingCounterId,
    [property: JsonPropertyName("estimatedClearingMinutes")] double EstimatedClearingMinutes,
    [property: JsonPropertyName("estimatedAvgWaitMinutes")] double EstimatedAvgWaitMinutes
);

public record CounterStateDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("label")] string Label,
    [property: JsonPropertyName("serviceType")] string ServiceType,
    [property: JsonPropertyName("isOpen")] bool IsOpen,
    [property: JsonPropertyName("staffAvailable")] bool StaffAvailable,
    [property: JsonPropertyName("currentTicketCode")] string? CurrentTicketCode,
    [property: JsonPropertyName("currentTicketId")] int? CurrentTicketId
);

public record BranchDashboardDto(
    [property: JsonPropertyName("branchId")] int BranchId,
    [property: JsonPropertyName("branchName")] string BranchName,
    [property: JsonPropertyName("location")] string Location,
    [property: JsonPropertyName("maxCapacity")] int MaxCapacity,
    [property: JsonPropertyName("occupancy")] int Occupancy,
    [property: JsonPropertyName("occupancyPercent")] double OccupancyPercent,
    [property: JsonPropertyName("crowdLevel")] string CrowdLevel,
    [property: JsonPropertyName("queueBookingBlocked")] bool QueueBookingBlocked,
    [property: JsonPropertyName("bookingBlockReason")] string? BookingBlockReason,
    [property: JsonPropertyName("crowdMediumStartsAtPercent")] int CrowdMediumStartsAtPercent,
    [property: JsonPropertyName("crowdHighStartsAtPercent")] int CrowdHighStartsAtPercent,
    [property: JsonPropertyName("overcrowdStartsAtPercent")] int OvercrowdStartsAtPercent,
    [property: JsonPropertyName("services")] IReadOnlyList<ServiceQueueStateDto> Services,
    [property: JsonPropertyName("counters")] IReadOnlyList<CounterStateDto> Counters,
    [property: JsonPropertyName("recentTickets")] IReadOnlyList<TicketSummaryDto> RecentTickets
);

public record TicketSummaryDto(
    [property: JsonPropertyName("ticketCode")] string TicketCode,
    [property: JsonPropertyName("serviceType")] string ServiceType,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("createdUtc")] DateTime CreatedUtc
);

public record JoinQueueResultDto(
    [property: JsonPropertyName("ticketCode")] string TicketCode,
    [property: JsonPropertyName("serviceType")] string ServiceType,
    [property: JsonPropertyName("positionInQueue")] int PositionInQueue,
    [property: JsonPropertyName("queueBookingBlocked")] bool QueueBookingBlocked,
    [property: JsonPropertyName("message")] string? Message
);

public record SimulatorGenerateRequest(
    [property: JsonPropertyName("count")] int Count,
    [property: JsonPropertyName("mode")] string Mode,
    [property: JsonPropertyName("serviceType")] BankServiceType? ServiceType,
    [property: JsonPropertyName("scenario")] string? Scenario
);

public record CallNextRequest([property: JsonPropertyName("counterId")] int CounterId);

public record TicketIdRequest([property: JsonPropertyName("ticketId")] int TicketId);

public record PatchCounterRequest(
    [property: JsonPropertyName("isOpen")] bool? IsOpen,
    [property: JsonPropertyName("staffAvailable")] bool? StaffAvailable,
    [property: JsonPropertyName("serviceType")] BankServiceType? ServiceType
);
