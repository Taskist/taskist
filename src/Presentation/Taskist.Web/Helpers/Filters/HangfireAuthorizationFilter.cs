using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Taskist.Core.Common;
using Taskist.Service.Authentication;

namespace Taskist.Web.Helpers.Filters;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    #region Fields

    protected readonly IServiceScopeFactory _scopeFactory;

    #endregion

    #region Ctor

    /// <summary>
    /// Hangfire resolves this filter once for the lifetime of the dashboard, so the
    /// scoped authentication service is resolved per request instead of being captured.
    /// </summary>
    public HangfireAuthorizationFilter(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    #endregion

    #region Methods

    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        using var scope = _scopeFactory.CreateScope();
        var authenticationService = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();

        //an anonymous request resolves to no user at all, so never dereference blindly
        var user = authenticationService.GetAuthenticatedUserAsync().GetAwaiter().GetResult();

        var isAdministrator = user?.UserRoles
            .Any(x => x.SystemName == Constant.AdministratorRoleName) ?? false;

        if (isAdministrator)
            return true;

        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
        httpContext.Response.ContentType = "text/html";
        httpContext.Response.WriteAsync("<h1>404 - Page Not Found</h1><p>You do not have access to the Hangfire dashboard.</p>");

        return false;
    }

    #endregion
}
