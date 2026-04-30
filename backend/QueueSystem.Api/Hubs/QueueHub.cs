using Microsoft.AspNetCore.SignalR;

namespace QueueSystem.Api.Hubs;

public class QueueHub : Hub
{
    public const string Path = "/hubs/queue";

    public static string BranchGroup(int branchId) => $"branch-{branchId}";

    public async Task SubscribeBranch(int branchId) =>
        await Groups.AddToGroupAsync(Context.ConnectionId, BranchGroup(branchId));
}
