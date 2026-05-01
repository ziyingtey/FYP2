using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using QueueSystem.Api.Auth;

namespace QueueSystem.Api.Hubs;

[Authorize]
public class QueueHub : Hub
{
    public const string Path = "/hubs/queue";

    public static string BranchGroup(int branchId) => $"branch-{branchId}";

    public async Task SubscribeBranch(int branchId)
    {
        var claim = Context.User?.FindFirst(AuthRoles.BranchIdClaim)?.Value;
        if (claim is null || !int.TryParse(claim, out var userBranch) || userBranch != branchId)
            throw new HubException("You cannot subscribe to this branch.");

        await Groups.AddToGroupAsync(Context.ConnectionId, BranchGroup(branchId));
    }
}
