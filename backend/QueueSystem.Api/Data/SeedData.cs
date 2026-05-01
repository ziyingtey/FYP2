using Microsoft.EntityFrameworkCore;
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

        if (!await db.Branches.AnyAsync())
        {
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

        await EnsureAdditionalBranchesAsync(db);
        await EnsureDemoCredentialsAsync(db);
    }

    private static async Task EnsureAdditionalBranchesAsync(AppDbContext db)
    {
        if (await db.Branches.AnyAsync(b => b.Name == "Mid Valley Megamall"))
            return;

        var b2 = new Branch
        {
            Name = "Mid Valley Megamall",
            Location = "Lingkaran Syed Putra, Mid Valley City, Kuala Lumpur",
            MaxCapacity = 52
        };
        var b3 = new Branch
        {
            Name = "Gurney Plaza Penang",
            Location = "Persiaran Gurney, George Town, Pulau Pinang",
            MaxCapacity = 68
        };
        db.Branches.AddRange(b2, b3);
        await db.SaveChangesAsync();

        db.StaffMembers.AddRange(
            new Staff { BranchId = b2.Id, Name = "Preeti Nair", Role = StaffRole.Staff, Email = "preeti@example.com" },
            new Staff { BranchId = b2.Id, Name = "Ravi Kumar", Role = StaffRole.Manager, Email = "ravi@example.com" },
            new Staff { BranchId = b3.Id, Name = "Hani Yusuf", Role = StaffRole.Staff, Email = "hani@example.com" },
            new Staff { BranchId = b3.Id, Name = "Najib Wahab", Role = StaffRole.Manager, Email = "najib@example.com" });
        await db.SaveChangesAsync();

        var s2 = await db.StaffMembers.Where(x => x.BranchId == b2.Id).OrderBy(x => x.Id).ToListAsync();
        var s2a = s2.First(x => x.Role == StaffRole.Staff);
        var s2b = s2.First(x => x.Role == StaffRole.Manager);
        var s3 = await db.StaffMembers.Where(x => x.BranchId == b3.Id).OrderBy(x => x.Id).ToListAsync();
        var s3a = s3.First(x => x.Role == StaffRole.Staff);
        var s3b = s3.First(x => x.Role == StaffRole.Manager);

        db.Counters.AddRange(
            new Counter { BranchId = b2.Id, ServiceId = ServiceIdMapping.General, StaffId = s2a.Id, Label = "MV — General", IsOpen = true, StaffAvailable = true },
            new Counter { BranchId = b2.Id, ServiceId = ServiceIdMapping.Card, StaffId = s2b.Id, Label = "MV — Card", IsOpen = true, StaffAvailable = true },
            new Counter { BranchId = b2.Id, ServiceId = ServiceIdMapping.Wealth, StaffId = null, Label = "MV — Wealth", IsOpen = false, StaffAvailable = true },
            new Counter { BranchId = b3.Id, ServiceId = ServiceIdMapping.General, StaffId = s3a.Id, Label = "Gurney — General", IsOpen = true, StaffAvailable = true },
            new Counter { BranchId = b3.Id, ServiceId = ServiceIdMapping.Card, StaffId = s3b.Id, Label = "Gurney — Card", IsOpen = true, StaffAvailable = true },
            new Counter { BranchId = b3.Id, ServiceId = ServiceIdMapping.Wealth, StaffId = null, Label = "Gurney — Wealth", IsOpen = true, StaffAvailable = true });
        await db.SaveChangesAsync();
    }

    private static async Task EnsureDemoCredentialsAsync(AppDbContext db)
    {
        foreach (var s in await db.StaffMembers.ToListAsync())
        {
            if (!string.IsNullOrEmpty(s.LoginEmail) && !string.IsNullOrEmpty(s.PasswordHash))
                continue;

            var (loginEmail, plainPassword) = s.Name switch
            {
                "Aisha Rahman" => ("aisha.staff@bds.demo", "StaffDemo#1"),
                "Marcus Lee" => ("marcus.manager@bds.demo", "ManagerDemo#1"),
                "Preeti Nair" => ("preeti.staff@bds.demo", "StaffDemo#1"),
                "Ravi Kumar" => ("ravi.manager@bds.demo", "ManagerDemo#1"),
                "Hani Yusuf" => ("hani.staff@bds.demo", "StaffDemo#1"),
                "Najib Wahab" => ("najib.manager@bds.demo", "ManagerDemo#1"),
                _ => ($"user{s.Id}@bds.demo", s.Role == StaffRole.Manager ? "ManagerDemo#1" : "StaffDemo#1")
            };

            s.LoginEmail = loginEmail;
            s.PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword);
        }

        await db.SaveChangesAsync();
    }
}
