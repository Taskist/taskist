using Taskist.Service.Authentication;
using Hangfire.Dashboard;

namespace Taskist.Web.Helpers.Filters
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        #region Fields

        protected readonly IAuthenticationService _authenticationService;

        #endregion

        #region Ctor

        public HangfireAuthorizationFilter(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        #endregion

        #region Methods

        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            var user = _authenticationService.GetAuthenticatedUserAsync().Result;
            if (user.UserRoles.Any(x => x.SystemName == "SystemAdministrator"))
                return true;

            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            httpContext.Response.ContentType = "text/html";
            httpContext.Response.WriteAsync("<h1>404 - Page Not Found</h1><p>You do not have access to the Hangfire dashboard.</p>");

            return false;
        }

        #endregion
    }
}
