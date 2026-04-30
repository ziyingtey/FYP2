namespace QueueSystem.Api.Models;

public class Counter
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }
    public string Label { get; set; } = "";
    public BankServiceType ServiceType { get; set; }
    public bool IsOpen { get; set; } = true;
    /// <summary>False during lunch break — counter appears closed for assignment.</summary>
    public bool StaffAvailable { get; set; } = true;
}
