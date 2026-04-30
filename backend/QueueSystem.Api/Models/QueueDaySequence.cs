namespace QueueSystem.Api.Models;

/// <summary>Per-branch, per-service daily ticket sequence.</summary>
public class QueueDaySequence
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public BankServiceType ServiceType { get; set; }
    public DateTime DateUtc { get; set; }
    public int LastNumber { get; set; }
}
