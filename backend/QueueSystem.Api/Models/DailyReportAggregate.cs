using System.ComponentModel.DataAnnotations.Schema;

namespace QueueSystem.Api.Models;

[Table("report")]
public class DailyReportAggregate
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }
    public DateTime ReportDate { get; set; }
    public int TotalCustomersServed { get; set; }
    public double AvgWaitMinutes { get; set; }
    public string? PeakHour { get; set; }
    public int? BusiestServiceId { get; set; }
    public BankService? BusiestService { get; set; }
    public DateTime GeneratedUtc { get; set; }
}
