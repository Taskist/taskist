using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using Scrutor;
using StackExchange.Profiling.Storage;
using System.Net;
using Taskist.Core.Caching;
using Taskist.Core.Common;
using Taskist.Core.Domain.Common;
using Taskist.Data;
using Taskist.Data.Repository;
using Taskist.Service.Masters;
using Taskist.Web.Helpers.Common;
using Taskist.Web.Helpers.Filters;
using Taskist.Web.Helpers.ModelBinding;
using Taskist.Web.Helpers.Routing;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault;

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = false;
    options.ValidateOnBuild = true;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddMvcCore();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DataProtectionKeys")))
    .SetApplicationName("Backlog");

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(180);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var connectionString = builder.Configuration.GetConnectionString("AppContext");

builder.Services.AddDbContext<ApplicationContext>(options =>
     options.UseLazyLoadingProxies()
     .UseSqlServer(connectionString)
     .EnableDetailedErrors()
     .EnableSensitiveDataLogging(true)
     .UseLoggerFactory(LoggerFactory.Create(builder => builder.AddDebug())));

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddTransient<ICacheManager, MemoryCacheManager>();
builder.Services.AddScoped<IWorkContext, WorkContext>();
builder.Services.AddSingleton<HangfireAuthorizationFilter>();

builder.Services.AddEasyCaching(option => { option.UseInMemory(builder.Configuration, "default", "easycahing:inmemory"); });

builder.Services.Scan(scan => scan
                .FromApplicationDependencies(a => a.FullName.StartsWith("Taskist"))
                .AddClasses(true)
                .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                .AsMatchingInterface((service, filter) => filter.Where(implementation => implementation.Name.Equals($"I{service.Name}", StringComparison.OrdinalIgnoreCase)))
                .WithScopedLifetime());

var settings = new List<Type>();
var assemblies = AppDomain.CurrentDomain.GetAssemblies();
foreach (var assembly in assemblies)
{
    try
    {
        var types = assembly.GetTypes()
       .Where(x => typeof(ISettings).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
       .ToList();
        settings.AddRange(types);
    }
    catch { }
}

foreach (var setting in settings)
{
    builder.Services.AddScoped(setting, serviceProvider =>
    {
        return serviceProvider.GetRequiredService<ISettingService>().LoadSettingAsync(setting).Result;
    });
}

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddRouting(options => options.LowercaseUrls = true);

var sentryEnabled = builder.Configuration.GetValue<bool>("Sentry:Enabled");

if (sentryEnabled)
{
    builder.WebHost.UseSentry(o =>
    {
        o.Dsn = builder.Configuration["Sentry:Dsn"];
        o.Debug = builder.Configuration.GetValue<bool>("Sentry:Debug");
        o.TracesSampleRate = builder.Configuration.GetValue<double>("Sentry:TracesSampleRate");
    });
}

var mvcBuilder = builder.Services.AddControllersWithViews(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
}).AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
});

builder.Services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddMvcCore(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    options.ModelMetadataDetailsProviders.Add(new MetadataProvider());
}).AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddAuthentication(a =>
{
    a.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    a.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie(options =>
{
    options.AccessDeniedPath = "/account/forbidden/";
    options.LoginPath = "/Login";
    options.SlidingExpiration = true;
});

#region Mini Profile

builder.Services.AddMiniProfiler(options =>
{
    options.RouteBasePath = "/profiler";
    (options.Storage as MemoryCacheStorage).CacheDuration = TimeSpan.FromMinutes(60);
    options.SqlFormatter = new StackExchange.Profiling.SqlFormatters.InlineFormatter();
    options.TrackConnectionOpenClose = true;
    options.ColorScheme = StackExchange.Profiling.ColorScheme.Auto;
    options.PopupDecimalPlaces = 1;
    options.EnableMvcFilterProfiling = true;
    options.EnableMvcViewProfiling = true;
});

#endregion

#region Hangfire

builder.Services.AddHangfire(config =>
{
    config.UseSqlServerStorage(connectionString, new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.FromSeconds(30), // Adjust polling interval
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    });
});

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 1;
    options.Queues = ["default"];
});


#endregion

if (builder.Environment.IsDevelopment())
{
    mvcBuilder.AddRazorRuntimeCompilation();
}

var app = builder.Build();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [app.Services.GetRequiredService<HangfireAuthorizationFilter>()]
});

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
app.UseStaticFiles();
app.UseExceptionHandler();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoint => RouteProvider.Configure(endpoint));

await app.RunAsync();
