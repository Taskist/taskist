namespace Taskist.Web.Helpers.Routing;

public static class RouteProvider
{
    public static IEndpointRouteBuilder Configure(IEndpointRouteBuilder routeBuilder)
    {
        #region Common

        routeBuilder.MapControllerRoute("PageNotFound", "PageNotFound", new { controller = "Common", Action = "PageNotFound" });
        routeBuilder.MapControllerRoute("AccessDenied", "AccessDenied", new { Controller = "Common", Action = "AccessDeniedView" });
        routeBuilder.MapControllerRoute("AccessDeniedPartialView", "AccessDeniedPartialView", new { Controller = "Security", Action = "AccessDeniedPartialView" });
        routeBuilder.MapControllerRoute("Error", "Error/{id}", new { Controller = "Common", Action = "Error", Id = 0 });
        routeBuilder.MapControllerRoute("ResetCache", "ResetCache", new { controller = "Common", Action = "ResetCache" });

        #endregion

        #region Anonymous

        routeBuilder.MapControllerRoute("Login", "Login", new { Controller = "Account", Action = "Login" });

        routeBuilder.MapControllerRoute("Logout", "Logout", new { Controller = "Account", Action = "Logout" });

        routeBuilder.MapControllerRoute("Activate", "Activate", new { Controller = "Account", Action = "Activate" });

        routeBuilder.MapControllerRoute("SetPassword", "setpassword", new { Controller = "Account", Action = "Activate" });

        routeBuilder.MapControllerRoute("ForgotPassword", "ForgotPassword", new { Controller = "Account", Action = "ForgotPassword" });

        #endregion

        #region Default

        routeBuilder.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

        #endregion

        return routeBuilder;
    }
}