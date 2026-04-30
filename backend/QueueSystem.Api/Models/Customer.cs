using System.ComponentModel.DataAnnotations.Schema;

namespace QueueSystem.Api.Models;

[Table("customer")]
public class Customer
{
    public int Id { get; set; }
    public int QueueTicketId { get; set; }
    public QueueTicket? QueueTicket { get; set; }
    public DateTime ArrivalUtc { get; set; }
    public bool IsSimulated { get; set; }
}
