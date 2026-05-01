using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace QueueSystem.Api.Auth;

/// <summary>Ensures route <c>branchId</c> matches JWT <see cref="AuthRoles.BranchIdClaim"/>.</summary>
public sealed class BranchRouteMatchesClaimFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any())
        {
            await next();
            return;
        }

        if (context.HttpContext.User.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (!context.RouteData.Values.TryGetValue("branchId", out var br) || br is null)
        {
            await next();
            return;
        }

        if (!int.TryParse(br.ToString(), out var routeBranch))
        {
            await next();
            return;
        }

        var claim = context.HttpContext.User.FindFirst(AuthRoles.BranchIdClaim)?.Value;
        if (claim is null || !int.TryParse(claim, out var userBranch) || userBranch != routeBranch)
        {
            context.Result = new ObjectResult("You do not have access to this branch.") { StatusCode = StatusCodes.Status403Forbidden };
            return;
        }

        await next();
    }
}
