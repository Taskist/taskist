using System.Net;
using Taskist.Web.Helpers.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables("TASKIST_");
ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault;

builder.Host.UseDefaultServiceProvider(options =>
{
    //scope validation catches scoped services captured by singletons
    options.ValidateScopes = builder.Environment.IsDevelopment();
    options.ValidateOnBuild = true;
});

var connectionString = builder.Configuration.GetConnectionString("AppContext");

if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException(
        "ConnectionStrings:AppContext is not configured. Set it through configuration, user-secrets " +
        "or the TASKIST_ConnectionStrings__AppContext environment variable.");

builder.Services
    .AddTaskistSecurity(builder.Configuration)
    .AddTaskistData(connectionString, builder.Environment)
    .AddTaskistCaching(builder.Configuration)
    .AddTaskistServices()
    .AddTaskistMvc(builder.Environment)
    .AddTaskistJobs(connectionString);

builder.AddTaskistMonitoring();

var app = builder.Build();

app.UseTaskistPipeline();

await app.RunAsync();
