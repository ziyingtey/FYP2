using System.ComponentModel.DataAnnotations.Schema;

namespace QueueSystem.Api.Models;

/// <summary>Per-branch, per-service daily ticket sequence.</summary>
[Table("queue_day_sequence")]
public class QueueDaySequence
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }
    public int ServiceId { get; set; }
    public BankService? Service { get; set; }
    public DateTime DateUtc { get; set; }
    public int LastNumber { get; set; }
}
