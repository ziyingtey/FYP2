namespace QueueSystem.Api.Models;

/// <summary>Maps API enum (0..2) to seeded <see cref="BankService"/> primary keys (1..3).</summary>
public static class ServiceIdMapping
{
    public const int General = 1;
    public const int Card = 2;
    public const int Wealth = 3;

    public static int FromApi(BankServiceType t) => (int)t + 1;

    public static BankServiceType ToApi(int serviceId) => serviceId switch
    {
        General => BankServiceType.General,
        Card => BankServiceType.Card,
        Wealth => BankServiceType.Wealth,
        _ => BankServiceType.General
    };

    public static string TicketPrefix(int serviceId) => serviceId switch
    {
        General => "G",
        Card => "C",
        Wealth => "W",
        _ => "Q"
    };
}
