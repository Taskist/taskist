using System.Security.Claims;
using Sentry;
using Sentry.AspNetCore;
using Sentry.Extensibility;

namespace Taskist.Web.Infrastructure;

public static class ErrorTrackingExtensions
{
    #region Methods

    public static WebApplicationBuilder AddErrorTracking(this WebApplicationBuilder builder)
    {
        var options = builder.Configuration.GetSection(ErrorTrackingOptions.SectionName).Get<ErrorTrackingOptions>() ?? new ErrorTrackingOptions();
        builder.Services.AddSingleton(options);

        if (!options.IsActive)
            return builder;

        var version = builder.Configuration["App:Version"];
        builder.WebHost.UseSentry((SentryAspNetCoreOptions sentry) =>
        {
            sentry.Dsn = options.Dsn!.Trim();
            sentry.Environment = string.IsNullOrWhiteSpace(options.Environment) ? builder.Environment.EnvironmentName : options.Environment.Trim();
            sentry.Release = string.IsNullOrWhiteSpace(version) ? null : $"taskist@{version}";
            sentry.TracesSampleRate = options.EffectiveTracesSampleRate;
            sentry.Debug = options.Debug;
            sentry.SendDefaultPii = false;
            sentry.MaxRequestBodySize = RequestSize.None;
            sentry.MinimumEventLevel = LogLevel.Error;
            sentry.MinimumBreadcrumbLevel = LogLevel.Information;
            sentry.SetBeforeSend((sentryEvent, _) => SentryEventScrubber.Scrub(sentryEvent));
            sentry.SetBeforeBreadcrumb((breadcrumb, _) => SentryEventScrubber.ScrubBreadcrumb(breadcrumb));
        });

        return builder;
    }

    public static WebApplication UseErrorTrackingUser(this WebApplication app)
    {
        if (!app.Services.GetRequiredService<ErrorTrackingOptions>().IsActive)
            return app;

        app.Use(async (context, next) =>
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
                SentrySdk.ConfigureScope(scope => scope.User = new SentryUser { Id = userId });

            await next();
        });

        return app;
    }

    #endregion
}
