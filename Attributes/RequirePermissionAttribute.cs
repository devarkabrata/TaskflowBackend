using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;
using TaskFlowBackend.Enums;
using TaskFlowBackend.Services.Interfaces;

namespace TaskFlowBackend.Attributes
{
    // Gates an action on the caller having a given permission for the team identified
    // by a route/action argument (defaults to "id", e.g. TeamController's {id:guid}).
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RequirePermissionAttribute : Attribute, IAsyncActionFilter
    {
        private readonly PermissionType _permission;
        private readonly string _teamIdArgumentName;

        public RequirePermissionAttribute(PermissionType permission, string teamIdArgumentName = "id")
        {
            _permission = permission;
            _teamIdArgumentName = teamIdArgumentName;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ActionArguments.TryGetValue(_teamIdArgumentName, out var rawTeamId) || rawTeamId is not Guid teamId)
                throw new InvalidOperationException(
                    $"[RequirePermission] could not resolve a Guid action argument named '{_teamIdArgumentName}'.");

            var userId = Guid.Parse(context.HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

            var permissionService = context.HttpContext.RequestServices.GetRequiredService<IPermissionService>();
            await permissionService.EnsureHasPermissionAsync(userId, teamId, _permission);

            await next();
        }
    }
}
