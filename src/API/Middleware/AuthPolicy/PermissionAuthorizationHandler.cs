using Microsoft.AspNetCore.Authorization;
using static Domain.Helper.PermissionProvider;

namespace API.Middleware.AuthPolicy;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    /// <summary>
    /// Makes a decision if authorization is allowed based on a specific requirement.
    /// </summary>
    /// <param name="context">The authorization context.</param>
    /// <param name="requirement">The requirement to evaluate.</param>
    /// <returns></returns>
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var actionClaim = context.User.Claims.Where(x => x.Type == nameof(Permission)).ToList();

        if (actionClaim.Count == 0 || !actionClaim.Any(x => x.Value.Contains(requirement.PermissionName)))
        {
            return Task.CompletedTask;
        }
        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}

public class PermissionRequirement(string permissionName) : IAuthorizationRequirement
{
    public string PermissionName { get; set; } = permissionName;
}
