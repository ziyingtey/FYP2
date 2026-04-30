using System.ComponentModel.DataAnnotations.Schema;

namespace QueueSystem.Api.Models;

[Table("simulation_run")]
public class SimulationRun
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }
    public int? StartedByStaffId { get; set; }
    public Staff? StartedByStaff { get; set; }
    public int RequestedCount { get; set; }
    public int GeneratedCount { get; set; }
    public string Mode { get; set; } = "";
    public int? FixedServiceId { get; set; }
    public BankService? FixedService { get; set; }
    public DateTime CreatedUtc { get; set; }
}
