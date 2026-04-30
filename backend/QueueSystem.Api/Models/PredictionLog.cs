using System.ComponentModel.DataAnnotations.Schema;

namespace QueueSystem.Api.Models;

[Table("prediction_log")]
public class PredictionLog
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }
    public int ServiceId { get; set; }
    public BankService? Service { get; set; }
    public double EstimatedAvgWaitMinutes { get; set; }
    public double EstimatedClearingMinutes { get; set; }
    public int QueueLength { get; set; }
    public int ActiveCounters { get; set; }
    public double AvgServiceMinutesInput { get; set; }
    public string Source { get; set; } = "";
    public DateTime TimestampUtc { get; set; }
}
