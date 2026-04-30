using System.ComponentModel.DataAnnotations.Schema;

namespace QueueSystem.Api.Models;

[Table("branch")]
public class Branch
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Location { get; set; } = "";
    /// <summary>Maximum people allowed in branch (waiting + being served).</summary>
    public int MaxCapacity { get; set; } = 80;

    /// <summary>Occupancy % at or above this value shows Medium (exclusive of High).</summary>
    public int CrowdMediumStartsAtPercent { get; set; } = 40;

    /// <summary>Occupancy % at or above this value shows High (exclusive of Overcrowded).</summary>
    public int CrowdHighStartsAtPercent { get; set; } = 70;

    /// <summary>Occupancy % at or above this value shows Overcrowded.</summary>
    public int OvercrowdStartsAtPercent { get; set; } = 100;
}
