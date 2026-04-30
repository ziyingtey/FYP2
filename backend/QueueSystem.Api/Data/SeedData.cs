using Microsoft.EntityFrameworkCore;
using QueueSystem.Api.Data;
using QueueSystem.Api.Models;

namespace QueueSystem.Api.Data;

public static class SeedData
{
    public static async Task EnsureSeedAsync(AppDbContext db)
    {
        if (!await db.BankServices.AnyAsync())
        {
            db.BankServices.AddRange(
                new BankService { Id = ServiceIdMapping.General, Code = "General", DisplayName = "General Banking", AvgServiceTimeMinutes = 8 },
                new BankService { Id = ServiceIdMapping.Card, Code = "Card", DisplayName = "Card Services", AvgServiceTimeMinutes = 6 },
                new BankService { Id = ServiceIdMapping.Wealth, Code = "Wealth", DisplayName = "Wealth", AvgServiceTimeMinutes = 12 });
            await db.SaveChangesAsync();
        }

        if (await db.Branches.AnyAsync()) return;

        var branch = new Branch
        {
            Name = "Main Branch",
            Location = "Kuala Lumpur City Centre",
            MaxCapacity = 80
        };
        db.Branches.Add(branch);
        await db.SaveChangesAsync();

        db.StaffMembers.AddRange(
            new Staff { BranchId = branch.Id, Name = "Aisha Rahman", Role = StaffRole.Staff, Email = "aisha@example.com" },
            new Staff { BranchId = branch.Id, Name = "Marcus Lee", Role = StaffRole.Manager, Email = "marcus@example.com" });
        await db.SaveChangesAsync();

        var staffList = await db.StaffMembers.Where(s => s.BranchId == branch.Id).OrderBy(s => s.Id).ToListAsync();
        var staffA = staffList.FirstOrDefault(s => s.Role == StaffRole.Staff);
        var staffB = staffList.FirstOrDefault(s => s.Role == StaffRole.Manager);

        db.Counters.AddRange(
            new Counter
            {
                BranchId = branch.Id,
                ServiceId = ServiceIdMapping.General,
                StaffId = staffA?.Id,
                Label = "Counter 1 — General",
                IsOpen = true,
                StaffAvailable = true
            },
            new Counter
            {
                BranchId = branch.Id,
                ServiceId = ServiceIdMapping.Card,
                StaffId = staffB?.Id,
                Label = "Counter 2 — Card",
                IsOpen = true,
                StaffAvailable = true
            },
            new Counter
            {
                BranchId = branch.Id,
                ServiceId = ServiceIdMapping.Wealth,
                StaffId = null,
                Label = "Counter 3 — Wealth",
                IsOpen = false,
                StaffAvailable = true
            });
        await db.SaveChangesAsync();
    }
}
