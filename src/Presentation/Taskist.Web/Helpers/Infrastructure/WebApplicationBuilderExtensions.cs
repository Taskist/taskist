namespace Taskist.Web.Helpers.Infrastructure;

/// <summary>
/// Host level wiring that has to run against the builder rather than the service collection.
/// </summary>
public static class WebApplicationBuilderExtensions
{
    #region Methods

    /// <summary>
    /// Enables Sentry error reporting when it is switched on in configuration.
    /// </summary>
    public static WebApplicationBuilder AddTaskistMonitoring(this WebApplicationBuilder builder)
    {
        if (!builder.Configuration.GetValue<bool>("Sentry:Enabled"))
            return builder;

        builder.WebHost.UseSentry(options =>
        {
            options.Dsn = builder.Configuration["Sentry:Dsn"];
            options.Debug = builder.Configuration.GetValue<bool>("Sentry:Debug");
            options.TracesSampleRate = builder.Configuration.GetValue<double>("Sentry:TracesSampleRate");
        });

        return builder;
    }

    #endregion
}
