using Microsoft.EntityFrameworkCore;
using QueueSystem.Api.Models;

namespace QueueSystem.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<BankService> BankServices => Set<BankService>();
    public DbSet<Staff> StaffMembers => Set<Staff>();
    public DbSet<Counter> Counters => Set<Counter>();
    public DbSet<QueueTicket> QueueTickets => Set<QueueTicket>();
    public DbSet<QueueDaySequence> QueueDaySequences => Set<QueueDaySequence>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<QueueAssignment> QueueAssignments => Set<QueueAssignment>();
    public DbSet<CrowdLog> CrowdLogs => Set<CrowdLog>();
    public DbSet<PredictionLog> PredictionLogs => Set<PredictionLog>();
    public DbSet<DailyReportAggregate> DailyReports => Set<DailyReportAggregate>();
    public DbSet<SimulationRun> SimulationRuns => Set<SimulationRun>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Branch>(e =>
        {
            e.ToTable("branch");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.Location).HasMaxLength(500);
        });

        modelBuilder.Entity<BankService>(e =>
        {
            e.ToTable("service");
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(64);
            e.Property(x => x.DisplayName).HasMaxLength(200);
            e.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<Staff>(e =>
        {
            e.ToTable("staff");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId);
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.Email).HasMaxLength(256);
        });

        modelBuilder.Entity<Counter>(e =>
        {
            e.ToTable("counter");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId);
            e.HasOne(x => x.Service).WithMany().HasForeignKey(x => x.ServiceId);
            e.HasOne(x => x.Staff).WithMany().HasForeignKey(x => x.StaffId).OnDelete(DeleteBehavior.SetNull);
            e.Property(x => x.Label).HasMaxLength(100);
        });

        modelBuilder.Entity<QueueTicket>(e =>
        {
            e.ToTable("queue");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.BranchId, x.Status, x.ServiceId });
            e.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId);
            e.HasOne(x => x.Service).WithMany().HasForeignKey(x => x.ServiceId);
            e.HasOne(x => x.Counter).WithMany().HasForeignKey(x => x.CounterId).OnDelete(DeleteBehavior.SetNull);
            e.Property(x => x.TicketCode).HasMaxLength(32);
        });

        modelBuilder.Entity<QueueDaySequence>(e =>
        {
            e.ToTable("queue_day_sequence");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.BranchId, x.ServiceId, x.DateUtc }).IsUnique();
            e.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId);
            e.HasOne(x => x.Service).WithMany().HasForeignKey(x => x.ServiceId);
        });

        modelBuilder.Entity<Customer>(e =>
        {
            e.ToTable("customer");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.QueueTicket).WithMany().HasForeignKey(x => x.QueueTicketId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.QueueTicketId).IsUnique();
        });

        modelBuilder.Entity<QueueAssignment>(e =>
        {
            e.ToTable("queue_assignment");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.QueueTicket).WithMany().HasForeignKey(x => x.QueueTicketId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Counter).WithMany().HasForeignKey(x => x.CounterId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.QueueTicketId, x.ReleasedUtc });
        });

        modelBuilder.Entity<CrowdLog>(e =>
        {
            e.ToTable("crowd_log");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId);
            e.Property(x => x.CrowdLevel).HasMaxLength(32);
        });

        modelBuilder.Entity<PredictionLog>(e =>
        {
            e.ToTable("prediction_log");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId);
            e.HasOne(x => x.Service).WithMany().HasForeignKey(x => x.ServiceId);
            e.Property(x => x.Source).HasMaxLength(32);
        });

        modelBuilder.Entity<DailyReportAggregate>(e =>
        {
            e.ToTable("report");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId);
            e.HasOne(x => x.BusiestService).WithMany().HasForeignKey(x => x.BusiestServiceId).OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(x => new { x.BranchId, x.ReportDate }).IsUnique();
            e.Property(x => x.PeakHour).HasMaxLength(64);
        });

        modelBuilder.Entity<SimulationRun>(e =>
        {
            e.ToTable("simulation_run");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId);
            e.HasOne(x => x.StartedByStaff).WithMany().HasForeignKey(x => x.StartedByStaffId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.FixedService).WithMany().HasForeignKey(x => x.FixedServiceId).OnDelete(DeleteBehavior.SetNull);
            e.Property(x => x.Mode).HasMaxLength(32);
        });
    }
}
