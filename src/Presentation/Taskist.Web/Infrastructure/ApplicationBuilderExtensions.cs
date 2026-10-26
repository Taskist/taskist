using Hangfire;
using Microsoft.AspNetCore.StaticFiles;
using Taskist.Web.Hubs;
using Taskist.Web.Helpers.Filters;
using Taskist.Web.Helpers.Routing;

namespace Taskist.Web.Infrastructure;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseApplicationPipeline(this WebApplication app)
    {
        var security = app.Services.GetRequiredService<HostSecurityOptions>();
        if (security.ForwardedHeaders.Enabled)
            app.UseForwardedHeaders();

        if (app.Environment.IsDevelopment())
        {
            app.UseMiniProfiler();
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            if (security.RequireHttpsCookies)
                app.UseHsts();
        }

        app.UseStatusCodePagesWithReExecute("/PageNotFound");
        if (security.RequireHttpsCookies)
            app.UseHttpsRedirection();
        app.UseWebOptimizer();

        var contentTypeProvider = new FileExtensionContentTypeProvider();
        contentTypeProvider.Mappings[".webmanifest"] = "application/manifest+json";
        app.UseStaticFiles(new StaticFileOptions
        {
            ContentTypeProvider = contentTypeProvider,
            OnPrepareResponse = context =>
            {
                if (context.Context.Request.Path.Equals("/js/sw.js", StringComparison.OrdinalIgnoreCase))
                {
                    context.Context.Response.Headers["Service-Worker-Allowed"] = "/";
                    context.Context.Response.Headers.CacheControl = "no-cache";
                }
            }
        });
        app.UseExceptionHandler();
        app.UseRouting();
        app.UseRateLimiter();
        app.UseSession();
        app.UseAuthentication();
        app.UseErrorTrackingUser();
        app.UseAuthorization();

        // After auth so the dashboard filter can see the authenticated user.
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = [app.Services.GetRequiredService<HangfireAuthorizationFilter>()],
            DashboardTitle = "Taskist — Background Jobs",
            AppPath = "/",
            DisplayStorageConnectionString = false
        });

        app.MapControllers();
        app.MapHub<AlertsHub>("/hubs/alerts");
        app.UseEndpoints(endpoint => RouteProvider.Configure(endpoint));

        return app;
    }
}
