using QueueSystem.Api.Models;

namespace QueueSystem.Api.Services;

/// <summary>Maps time-of-day / stress scenarios to volume and service mix for realistic demos.</summary>
public static class SimulationScenarioHelper
{
    public static int AdjustCount(int requested, string? scenario)
    {
        if (string.IsNullOrWhiteSpace(scenario)) return Math.Clamp(requested, 1, 500);
        var s = scenario.Trim().ToLowerInvariant();
        var mult = s switch
        {
            "morning" => 0.35,
            "midday" => 0.65,
            "peak" or "lunch" => 1.55,
            "evening" => 0.8,
            "stress" or "overcrowd" => 2.1,
            _ => 1.0
        };
        return Math.Clamp((int)Math.Round(requested * mult), 1, 500);
    }

    /// <summary>Pick next service for random mode using scenario bias.</summary>
    public static BankServiceType PickRandomService(Random rnd, string? scenario)
    {
        var s = scenario?.Trim().ToLowerInvariant() ?? "";
        var r = rnd.NextDouble();
        return s switch
        {
            "morning" or "evening" => r switch
            {
                < 0.45 => BankServiceType.General,
                < 0.75 => BankServiceType.Card,
                _ => BankServiceType.Wealth
            },
            "peak" or "lunch" => r switch
            {
                < 0.68 => BankServiceType.General,
                < 0.9 => BankServiceType.Card,
                _ => BankServiceType.Wealth
            },
            "stress" or "overcrowd" => r switch
            {
                < 0.55 => BankServiceType.General,
                < 0.85 => BankServiceType.Card,
                _ => BankServiceType.Wealth
            },
            _ => (BankServiceType)rnd.Next(0, 3)
        };
    }
}
