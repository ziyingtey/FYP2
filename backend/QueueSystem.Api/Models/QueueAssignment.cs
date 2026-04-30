using System.ComponentModel.DataAnnotations.Schema;

namespace QueueSystem.Api.Models;

[Table("queue_assignment")]
public class QueueAssignment
{
    public int Id { get; set; }
    public int QueueTicketId { get; set; }
    public QueueTicket? QueueTicket { get; set; }
    public int CounterId { get; set; }
    public Counter? Counter { get; set; }
    public DateTime AssignedUtc { get; set; }
    public DateTime? ReleasedUtc { get; set; }
    public QueueAssignmentKind Kind { get; set; }
}
