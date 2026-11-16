using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using Taskist.Core.Common;

namespace Taskist.Web.Helpers.Attributes;

public class ValidateUserIdAttribute : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        var path = context.HttpContext.Request.Path;

        if (path.StartsWithSegments("/Account", StringComparison.OrdinalIgnoreCase))
            return;

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        var workContext = context.HttpContext.RequestServices.GetRequiredService<IWorkContext>();
        if (string.IsNullOrEmpty(userId) || await workContext.GetCurrentUserAsync() == null)
        {
            await context.HttpContext.SignOutAsync();

            context.Result = new RedirectToRouteResult(new RouteValueDictionary
            {
                { "controller", "Account" },
                { "action", "Login" }
            });
        }
    }
}
