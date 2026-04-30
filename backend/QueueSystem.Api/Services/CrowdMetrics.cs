namespace QueueSystem.Api.Services;

public static class CrowdMetrics
{
    public static string Level(double occupancyPercent)
    {
        if (occupancyPercent >= 100) return "Overcrowded";
        if (occupancyPercent >= 70) return "High";
        if (occupancyPercent >= 40) return "Medium";
        return "Low";
    }
}
