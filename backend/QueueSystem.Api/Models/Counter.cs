using System.ComponentModel.DataAnnotations.Schema;

namespace QueueSystem.Api.Models;

[Table("counter")]
public class Counter
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }
    public int ServiceId { get; set; }
    public BankService? Service { get; set; }
    public int? StaffId { get; set; }
    public Staff? Staff { get; set; }
    public string Label { get; set; } = "";
    public bool IsOpen { get; set; } = true;
    /// <summary>False during lunch break — counter appears closed for assignment.</summary>
    public bool StaffAvailable { get; set; } = true;
}
