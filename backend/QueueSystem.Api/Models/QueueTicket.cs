namespace QueueSystem.Api.Models;

public class QueueTicket
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }
    public BankServiceType ServiceType { get; set; }
    public string TicketCode { get; set; } = "";
    public TicketStatus Status { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime? CalledUtc { get; set; }
    /// <summary>When set, this waiting ticket is served before others of the same service (recall).</summary>
    public DateTime? RecalledUtc { get; set; }
    public DateTime? CompletedUtc { get; set; }
    public int? CounterId { get; set; }
    public Counter? Counter { get; set; }
}
