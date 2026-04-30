using System.ComponentModel.DataAnnotations.Schema;

namespace QueueSystem.Api.Models;

/// <summary>Catalog row for queue service types (General / Card / Wealth).</summary>
[Table("service")]
public class BankService
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public int AvgServiceTimeMinutes { get; set; }
}
