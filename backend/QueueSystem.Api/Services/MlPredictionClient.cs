using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace QueueSystem.Api.Services;

public interface IMlPredictionClient
{
    Task<MlPredictionResult> PredictAsync(MlPredictionRequest request, CancellationToken ct = default);
}

public record MlPredictionRequest(
    [property: JsonPropertyName("queue_length")] int QueueLength,
    [property: JsonPropertyName("active_counters")] int ActiveCounters,
    [property: JsonPropertyName("service_type")] string ServiceType,
    [property: JsonPropertyName("avg_service_minutes")] double AvgServiceMinutes
);

public record MlPredictionResult(
    [property: JsonPropertyName("estimated_avg_wait_minutes")] double EstimatedAvgWaitMinutes,
    [property: JsonPropertyName("estimated_clearing_minutes")] double EstimatedClearingMinutes
);

public class MlPredictionClient : IMlPredictionClient
{
    private readonly HttpClient _http;
    private readonly ILogger<MlPredictionClient> _logger;
    private readonly IConfiguration _configuration;

    public MlPredictionClient(HttpClient http, ILogger<MlPredictionClient> logger, IConfiguration configuration)
    {
        _http = http;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<MlPredictionResult> PredictAsync(MlPredictionRequest request, CancellationToken ct = default)
    {
        var baseUrl = _configuration["MlService:BaseUrl"]?.TrimEnd('/') ?? "http://localhost:8000";
        try
        {
            using var response = await _http.PostAsJsonAsync($"{baseUrl}/predict", request, ct);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadFromJsonAsync<MlPredictionResult>(cancellationToken: ct);
            if (body != null) return body;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ML service unavailable; using heuristic fallback.");
        }

        return HeuristicFallback(request);
    }

    private static MlPredictionResult HeuristicFallback(MlPredictionRequest r)
    {
        var counters = Math.Max(1, r.ActiveCounters);
        var avg = r.AvgServiceMinutes > 0 ? r.AvgServiceMinutes : 5.0;
        var clearing = r.QueueLength / (double)counters * avg;
        var avgWait = clearing / 2.0;
        return new MlPredictionResult(Math.Round(avgWait, 1), Math.Round(clearing, 1));
    }
}
