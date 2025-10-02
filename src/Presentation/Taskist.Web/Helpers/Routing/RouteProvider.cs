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
        routeBuilder.MapControllerRoute("SetActiveProject", "SetActiveProject/{id}", new { Controller = "Common", Action = "SetActiveProject", Id = 0 });

        #endregion

        #region Anonymous

        routeBuilder.MapControllerRoute("Login", "Login", new { Controller = "Account", Action = "Login" });

        routeBuilder.MapControllerRoute("Logout", "Logout", new { Controller = "Account", Action = "Logout" });

        routeBuilder.MapControllerRoute("Activate", "Activate", new { Controller = "Account", Action = "Activate" });

        #endregion

        #region Default

        routeBuilder.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

        #endregion

        return routeBuilder;
    }
}