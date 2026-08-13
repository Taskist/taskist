using System.Reflection;
using System.Threading.RateLimiting;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Scrutor;
using StackExchange.Profiling.Storage;
using Taskist.Core.Caching;
using Taskist.Core.Common;
using Taskist.Core.Domain.Common;
using Taskist.Data;
using Taskist.Data.Repository;
using Taskist.Service.Masters;
using Taskist.Web.Helpers.Common;
using Taskist.Web.Helpers.Filters;
using Taskist.Web.Helpers.ModelBinding;

namespace Taskist.Web.Helpers.Infrastructure;

/// <summary>
/// Groups the application service registrations so Program.cs stays declarative.
/// </summary>
public static class ServiceCollectionExtensions
{
    #region Configuration

    /// <summary>
    /// Binds and eagerly validates the security options.
    /// </summary>
    public static IServiceCollection AddTaskistSecurity(this IServiceCollection services,
        IConfiguration configuration)
    {
        //behind a TLS terminating proxy the request arrives as plain HTTP; without this
        //the secure cookies below are never sent and sign-in silently fails
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

            //the proxy is not known ahead of time in a container deployment
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        services.AddOptions<SecurityOptions>()
            .Bind(configuration.GetSection(SecurityOptions.SectionName))
            .Validate(options =>
            {
                options.Validate();
                return true;
            })
            .ValidateOnStart();

        services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(
                Directory.GetCurrentDirectory(), "App_Data", "DataProtectionKeys")))
            .SetApplicationName("Taskist");

        var securityOptions = configuration.GetSection(SecurityOptions.SectionName).Get<SecurityOptions>()
            ?? new SecurityOptions();

        var cookieSecurePolicy = securityOptions.RequireHttpsCookies
            ? CookieSecurePolicy.Always
            : CookieSecurePolicy.SameAsRequest;

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        }).AddCookie(options =>
        {
            options.AccessDeniedPath = "/account/forbidden/";
            options.LoginPath = "/Login";
            options.SlidingExpiration = true;
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = cookieSecurePolicy;
        });

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            //throttles credential submission per client address
            options.AddPolicy(WebConstant.AuthRateLimitPolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));
        });

        return services;
    }

    #endregion

    #region Data

    /// <summary>
    /// Registers the database context and the repository abstraction.
    /// </summary>
    public static IServiceCollection AddTaskistData(this IServiceCollection services,
        string connectionString,
        IWebHostEnvironment environment)
    {
        services.AddDbContext<ApplicationContext>(options =>
        {
            options.UseLazyLoadingProxies()
                .UseSqlServer(connectionString)
                .UseLoggerFactory(LoggerFactory.Create(logging => logging.AddDebug()));

            //these expose parameter values (including credentials) and must stay out of production
            if (environment.IsDevelopment())
            {
                options.EnableDetailedErrors()
                    .EnableSensitiveDataLogging();
            }
        });

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        return services;
    }

    #endregion

    #region Caching

    /// <summary>
    /// Registers the in-memory cache and session state.
    /// </summary>
    public static IServiceCollection AddTaskistCaching(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDistributedMemoryCache();

        var securityOptions = configuration.GetSection(SecurityOptions.SectionName).Get<SecurityOptions>()
            ?? new SecurityOptions();

        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(180);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = securityOptions.RequireHttpsCookies
                ? CookieSecurePolicy.Always
                : CookieSecurePolicy.SameAsRequest;
        });

        services.AddEasyCaching(option =>
            option.UseInMemory(configuration, "default", "easycahing:inmemory"));

        services.AddTransient<ICacheManager, MemoryCacheManager>();

        return services;
    }

    #endregion

    #region Application services

    /// <summary>
    /// Discovers the Taskist services by convention and registers the settings classes.
    /// </summary>
    public static IServiceCollection AddTaskistServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped<IWorkContext, WorkContext>();
        services.AddSingleton<HangfireAuthorizationFilter>();

        services.Scan(scan => scan
            .FromApplicationDependencies(a => a.FullName.StartsWith("Taskist"))
            .AddClasses(true)
            .UsingRegistrationStrategy(RegistrationStrategy.Skip)
            .AsMatchingInterface((service, filter) => filter.Where(implementation =>
                implementation.Name.Equals($"I{service.Name}", StringComparison.OrdinalIgnoreCase)))
            .WithScopedLifetime());

        RegisterSettings(services);

        return services;
    }

    /// <summary>
    /// Registers every ISettings implementation so it can be injected directly.
    /// </summary>
    private static void RegisterSettings(IServiceCollection services)
    {
        var settings = new List<Type>();

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                settings.AddRange(assembly.GetTypes()
                    .Where(x => typeof(ISettings).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract));
            }
            catch (ReflectionTypeLoadException)
            {
                //a dependency that cannot be fully loaded contributes no settings
            }
        }

        foreach (var setting in settings)
        {
            services.AddScoped(setting, serviceProvider =>
                serviceProvider.GetRequiredService<ISettingService>().LoadSettingAsync(setting).Result);
        }
    }

    #endregion

    #region MVC

    /// <summary>
    /// Registers MVC, validation, mapping and the diagnostics used per environment.
    /// </summary>
    public static IServiceCollection AddTaskistMvc(this IServiceCollection services,
        IWebHostEnvironment environment)
    {
        services.AddRouting(options => options.LowercaseUrls = true);

        var mvcBuilder = services.AddControllersWithViews(options =>
        {
            options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
        }).AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.ContractResolver = new DefaultContractResolver();
        });

        services.AddMvcCore(options =>
        {
            options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            options.ModelMetadataDetailsProviders.Add(new MetadataProvider());
        }).AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        });

        services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();
        services.AddValidatorsFromAssemblyContaining<Program>();

        services.AddAutoMapper(typeof(Program));

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        if (environment.IsDevelopment())
        {
            mvcBuilder.AddRazorRuntimeCompilation();
            AddProfiler(services);
        }

        return services;
    }

    /// <summary>
    /// The profiler exposes executed SQL, so it is only wired up outside production.
    /// </summary>
    private static void AddProfiler(IServiceCollection services)
    {
        services.AddMiniProfiler(options =>
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
    }

    #endregion

    #region Background jobs

    /// <summary>
    /// Registers Hangfire and its worker.
    /// </summary>
    public static IServiceCollection AddTaskistJobs(this IServiceCollection services, string connectionString)
    {
        services.AddHangfire(config =>
        {
            config.UseSqlServerStorage(connectionString, new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.FromSeconds(30),
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            });
        });

        services.AddHangfireServer(options =>
        {
            options.WorkerCount = 1;
            options.Queues = ["default"];
        });

        return services;
    }

    #endregion
}
