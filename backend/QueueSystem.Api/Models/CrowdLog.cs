using System.ComponentModel.DataAnnotations.Schema;

namespace QueueSystem.Api.Models;

[Table("crowd_log")]
public class CrowdLog
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }
    public int TotalCustomers { get; set; }
    public string CrowdLevel { get; set; } = "";
    public DateTime TimestampUtc { get; set; }
}
