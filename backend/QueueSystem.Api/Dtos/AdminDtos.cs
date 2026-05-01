using System.Text.Json.Serialization;
using QueueSystem.Api.Models;

namespace QueueSystem.Api.Dtos;

public record BranchDetailDto(
    [property: JsonPropertyName("branchId")] int BranchId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("location")] string Location,
    [property: JsonPropertyName("maxCapacity")] int MaxCapacity,
    [property: JsonPropertyName("crowdMediumStartsAtPercent")] int CrowdMediumStartsAtPercent,
    [property: JsonPropertyName("crowdHighStartsAtPercent")] int CrowdHighStartsAtPercent,
    [property: JsonPropertyName("overcrowdStartsAtPercent")] int OvercrowdStartsAtPercent
);

public record PatchBranchSettingsRequest(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("location")] string? Location,
    [property: JsonPropertyName("maxCapacity")] int? MaxCapacity,
    [property: JsonPropertyName("crowdMediumStartsAtPercent")] int? CrowdMediumStartsAtPercent,
    [property: JsonPropertyName("crowdHighStartsAtPercent")] int? CrowdHighStartsAtPercent,
    [property: JsonPropertyName("overcrowdStartsAtPercent")] int? OvercrowdStartsAtPercent
);

public record StaffListItemDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("email")] string? Email,
    [property: JsonPropertyName("assignedCounterLabel")] string? AssignedCounterLabel
);

public record ServiceCatalogDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("displayName")] string DisplayName,
    [property: JsonPropertyName("avgServiceTimeMinutes")] int AvgServiceTimeMinutes
);

public record CrowdLogDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("totalCustomers")] int TotalCustomers,
    [property: JsonPropertyName("crowdLevel")] string CrowdLevel,
    [property: JsonPropertyName("timestampUtc")] DateTime TimestampUtc
);

public record PredictionLogDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("serviceCode")] string ServiceCode,
    [property: JsonPropertyName("queueLength")] int QueueLength,
    [property: JsonPropertyName("activeCounters")] int ActiveCounters,
    [property: JsonPropertyName("estimatedAvgWaitMinutes")] double EstimatedAvgWaitMinutes,
    [property: JsonPropertyName("estimatedClearingMinutes")] double EstimatedClearingMinutes,
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("timestampUtc")] DateTime TimestampUtc
);

public record SimulationRunDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("requestedCount")] int RequestedCount,
    [property: JsonPropertyName("generatedCount")] int GeneratedCount,
    [property: JsonPropertyName("mode")] string Mode,
    [property: JsonPropertyName("scenario")] string? Scenario,
    [property: JsonPropertyName("fixedServiceCode")] string? FixedServiceCode,
    [property: JsonPropertyName("createdUtc")] DateTime CreatedUtc
);

public record BranchSummaryDto(
    [property: JsonPropertyName("branchId")] int BranchId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("location")] string Location,
    [property: JsonPropertyName("maxCapacity")] int MaxCapacity,
    [property: JsonPropertyName("occupancy")] int Occupancy,
    [property: JsonPropertyName("occupancyPercent")] double OccupancyPercent,
    [property: JsonPropertyName("crowdLevel")] string CrowdLevel,
    [property: JsonPropertyName("queueBookingBlocked")] bool QueueBookingBlocked
);

public record TicketHistoryDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("ticketCode")] string TicketCode,
    [property: JsonPropertyName("serviceCode")] string ServiceCode,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("createdUtc")] DateTime CreatedUtc,
    [property: JsonPropertyName("calledUtc")] DateTime? CalledUtc,
    [property: JsonPropertyName("completedUtc")] DateTime? CompletedUtc,
    [property: JsonPropertyName("counterId")] int? CounterId,
    [property: JsonPropertyName("isSimulated")] bool IsSimulated
);
