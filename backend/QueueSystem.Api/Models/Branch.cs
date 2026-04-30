namespace QueueSystem.Api.Models;

public class Branch
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    /// <summary>Maximum people allowed in branch (waiting + being served).</summary>
    public int MaxCapacity { get; set; } = 80;
}
