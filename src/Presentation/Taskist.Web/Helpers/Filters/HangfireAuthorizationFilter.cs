using Taskist.Core.Common;
using Taskist.Service.Security;
using Hangfire.Dashboard;

namespace Taskist.Web.Helpers.Filters
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            if (httpContext.User?.Identity?.IsAuthenticated == true)
            {
                // Administrators always have access; other roles need the ManageJobs permission.
                if (httpContext.User.IsInRole(Constant.AdministratorRoleName))
                    return true;

                var permissionService = httpContext.RequestServices.GetService<IPermissionService>();
                if (permissionService != null
                    && permissionService.AuthorizeAsync(PermissionProvider.ManageJobs).GetAwaiter().GetResult())
                    return true;
            }

            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            httpContext.Response.ContentType = "text/html";
            httpContext.Response.WriteAsync("<h1>404 - Page Not Found</h1><p>You do not have access to the Hangfire dashboard.</p>");

            return false;
        }
    }
}
