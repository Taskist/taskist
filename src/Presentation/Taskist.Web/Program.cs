using System.Net;
using Taskist.Core.Domain.Settings;
using Taskist.Service;
using Taskist.Web.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault;

builder.AddErrorTracking();

builder.Host.UseDefaultServiceProvider(options =>
{
	options.ValidateScopes = false;
	options.ValidateOnBuild = true;
});

builder.Services
	.AddWebContext()
	.AddHostSecurity(builder.Configuration)
	.AddDataProtectionAndSession(builder.Environment, builder.Configuration)
	.AddPersistence(builder.Configuration, builder.Environment)
	.AddApplicationCaching()
	.AddApplicationServices()
	.AddAppUrl(builder.Configuration)
	.AddApplicationSettings()
	.AddObjectMapping()
	.AddAssetPipeline()
	.AddPresentation()
	.AddAppAuthentication()
	.AddProfiling()
	.AddBackgroundJobs(builder.Configuration, startServer: !builder.Environment.IsDevelopment());

builder.Services.AddSingleton(new Taskist.Core.Configuration.BackgroundJobMode { ServerEnabled = !builder.Environment.IsDevelopment() });

var app = builder.Build();

app.LogSecurityWarnings();
app.LogErrorTrackingStatus();

await GitStartup.UpgradeLegacySettingsAsync(app.Services);

app.UseApplicationPipeline();

if (!app.Environment.IsDevelopment())
{
	RecurringJobScheduler.RegisterLogPurge();
	RecurringJobScheduler.RegisterGitJobs();
}

await app.RunAsync();
