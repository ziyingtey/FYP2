using Microsoft.EntityFrameworkCore;
using QueueSystem.Api.Models;

namespace QueueSystem.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Counter> Counters => Set<Counter>();
    public DbSet<QueueTicket> QueueTickets => Set<QueueTicket>();
    public DbSet<QueueDaySequence> QueueDaySequences => Set<QueueDaySequence>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Branch>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200);
        });

        modelBuilder.Entity<Counter>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId);
            e.Property(x => x.Label).HasMaxLength(100);
        });

        modelBuilder.Entity<QueueTicket>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.BranchId, x.Status, x.ServiceType });
            e.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId);
            e.HasOne(x => x.Counter).WithMany().HasForeignKey(x => x.CounterId);
            e.Property(x => x.TicketCode).HasMaxLength(32);
        });

        modelBuilder.Entity<QueueDaySequence>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.BranchId, x.ServiceType, x.DateUtc }).IsUnique();
        });
    }
}
