namespace QueueSystem.Api.Services;

public static class CrowdMetrics
{
    /// <param name="mediumStartsAt">Inclusive lower bound for Medium band (e.g. 40).</param>
    /// <param name="highStartsAt">Inclusive lower bound for High band (e.g. 70).</param>
    /// <param name="overcrowdStartsAt">Inclusive lower bound for Overcrowded (e.g. 100).</param>
    public static string Level(double occupancyPercent, int mediumStartsAt, int highStartsAt, int overcrowdStartsAt)
    {
        if (occupancyPercent >= overcrowdStartsAt) return "Overcrowded";
        if (occupancyPercent >= highStartsAt) return "High";
        if (occupancyPercent >= mediumStartsAt) return "Medium";
        return "Low";
    }
}
