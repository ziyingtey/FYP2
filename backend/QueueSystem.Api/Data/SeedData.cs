using Microsoft.EntityFrameworkCore;
using QueueSystem.Api.Data;
using QueueSystem.Api.Models;

namespace QueueSystem.Api.Data;

public static class SeedData
{
    public static async Task EnsureSeedAsync(AppDbContext db)
    {
        if (await db.Branches.AnyAsync()) return;

        var branch = new Branch { Name = "Main Branch", MaxCapacity = 80 };
        db.Branches.Add(branch);
        await db.SaveChangesAsync();

        db.Counters.AddRange(
            new Counter { BranchId = branch.Id, Label = "Counter 1 — General", ServiceType = BankServiceType.General, IsOpen = true },
            new Counter { BranchId = branch.Id, Label = "Counter 2 — Card", ServiceType = BankServiceType.Card, IsOpen = true },
            new Counter { BranchId = branch.Id, Label = "Counter 3 — Wealth", ServiceType = BankServiceType.Wealth, IsOpen = false }
        );
        await db.SaveChangesAsync();
    }
}
