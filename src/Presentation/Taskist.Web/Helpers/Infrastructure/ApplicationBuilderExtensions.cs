using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Taskist.Web.Helpers.Filters;
using Taskist.Web.Helpers.Routing;

namespace Taskist.Web.Helpers.Infrastructure;

/// <summary>
/// Builds the HTTP pipeline. Middleware order is significant and is kept in one place.
/// </summary>
public static class ApplicationBuilderExtensions
{
	#region Methods

	public static WebApplication UseTaskistPipeline(this WebApplication app)
	{
		//must run before anything that inspects the scheme or the client address
		app.UseForwardedHeaders();

		if (app.Environment.IsDevelopment())
		{
			app.UseMiniProfiler();
			app.UseDeveloperExceptionPage();
		}
		else
		{
			app.UseExceptionHandler("/Error");
			app.UseHsts();
		}

		app.UseStatusCodePagesWithReExecute("/PageNotFound");
		app.UseHttpsRedirection();
		app.UseSecurityHeaders();

		//.webmanifest is not a known static-file type by default, so register it
		var contentTypeProvider = new FileExtensionContentTypeProvider();
		contentTypeProvider.Mappings[".webmanifest"] = "application/manifest+json";
		app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = contentTypeProvider });
		app.UseRouting();
		app.UseRateLimiter();
		app.UseSession();
		app.UseAuthentication();
		app.UseAuthorization();

		//registered after authentication so the dashboard filter sees the signed-in user
		app.UseHangfireDashboard("/hangfire", new DashboardOptions
		{
			Authorization = [app.Services.GetRequiredService<HangfireAuthorizationFilter>()]
		});

		app.UseEndpoints(endpoint => RouteProvider.Configure(endpoint));

		return app;
	}

	#endregion

	#region Utilities

	/// <summary>
	/// Applies the response headers that harden the browser side of the application.
	/// </summary>
	private static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
	{
		return app.Use(async (context, next) =>
		{
			var headers = context.Response.Headers;

			headers.XContentTypeOptions = "nosniff";
			headers.XFrameOptions = "SAMEORIGIN";
			headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

			await next();
		});
	}

	#endregion
}
