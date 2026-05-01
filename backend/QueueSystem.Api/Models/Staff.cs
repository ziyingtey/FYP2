using System.ComponentModel.DataAnnotations.Schema;

namespace QueueSystem.Api.Models;

[Table("staff")]
public class Staff
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }
    public string Name { get; set; } = "";
    public StaffRole Role { get; set; }
    public string? Email { get; set; }
    /// <summary>Unique login for branch portal (staff / manager).</summary>
    public string? LoginEmail { get; set; }
    public string? PasswordHash { get; set; }
}
