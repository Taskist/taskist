using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Serialization;
using StackExchange.Profiling.Storage;
using System.Reflection;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Taskist.Core.Common;
using Taskist.Core.Configuration;
using Taskist.Core.Domain.Common;
using Taskist.Core.Enums;
using Taskist.Core.Security;
using Taskist.Data;
using Taskist.Data.Repository;
using Taskist.Service.Common;
using Taskist.Service.Masters;
using Taskist.Service.Users;
using Taskist.Web.Helpers.Common;
using Taskist.Web.Helpers.Filters;
using Taskist.Web.Helpers.ModelBinding;
using Taskist.Web.Infrastructure.Realtime;

namespace Taskist.Web.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebContext(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddDistributedMemoryCache();
        services.AddScoped<IWorkContext, WorkContext>();
        services.AddScoped<Helpers.Appearance.IAppearanceService, Helpers.Appearance.AppearanceService>();
        services.AddScoped<Helpers.Reports.IReportLayoutService, Helpers.Reports.ReportLayoutService>();
        services.AddScoped<Helpers.Wiki.IWikiAccessService, Helpers.Wiki.WikiAccessService>();
        services.AddSingleton<Helpers.Documents.IUploadTokenService, Helpers.Documents.UploadTokenService>();

        return services;
    }

    public static IServiceCollection AddDataProtectionAndSession(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
        var section = configuration.GetSection("DataProtection");
        var keysPath = section["KeysPath"];
        if (string.IsNullOrWhiteSpace(keysPath))
            keysPath = Path.Combine(environment.ContentRootPath, "App_Data", "DataProtectionKeys");

        EnsureWritableDirectory(keysPath);

        var builder = services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
            .SetApplicationName("Backlog");

        var method = "none";
        var certificatePath = section["CertificatePath"];
        var certificateThumbprint = section["CertificateThumbprint"];
        if (!string.IsNullOrWhiteSpace(certificatePath))
        {
            var certificate = System.Security.Cryptography.X509Certificates.X509CertificateLoader.LoadPkcs12FromFile(
                certificatePath, section["CertificatePassword"]);
            builder.ProtectKeysWithCertificate(certificate);
            method = "certificate";
        }
        else if (!string.IsNullOrWhiteSpace(certificateThumbprint))
        {
            builder.ProtectKeysWithCertificate(certificateThumbprint);
            method = "certificate";
        }
        else if (section.GetValue<bool>("ProtectWithDpapi") && OperatingSystem.IsWindows())
        {
            builder.ProtectKeysWithDpapi(protectToLocalMachine: section.GetValue<bool>("DpapiLocalMachine"));
            method = "dpapi";
        }

        services.AddSingleton(new KeyRingProtectionInfo { ProtectedAtRest = method != "none", Method = method, KeysPath = keysPath });
        services.AddSingleton<ISecretProtector, SecretProtector>();

        services.AddSession(options =>
                {
                    options.IdleTimeout = TimeSpan.FromMinutes(180);
                    options.Cookie.HttpOnly = true;
                    options.Cookie.IsEssential = true;
                });

        return services;
    }

    public static IServiceCollection AddHostSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        var security = configuration.GetSection(HostSecurityOptions.SectionName).Get<HostSecurityOptions>() ?? new HostSecurityOptions();
        services.AddSingleton(security);

        var authCookiePolicy = security.RequireHttpsCookies ? CookieSecurePolicy.Always : CookieSecurePolicy.SameAsRequest;
        services.PostConfigure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options => options.Cookie.SecurePolicy = authCookiePolicy);
        services.PostConfigure<SessionOptions>(options => options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest);
        services.PostConfigure<AntiforgeryOptions>(options => options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest);

        if (security.ForwardedHeaders.Enabled)
        {
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

                foreach (var proxy in security.ForwardedHeaders.KnownProxies)
                {
                    if (!IPAddress.TryParse(proxy, out var address))
                        throw new InvalidOperationException($"Security:ForwardedHeaders:KnownProxies contains '{proxy}', which is not an IP address.");
                    options.KnownProxies.Add(address);
                }

                foreach (var network in security.ForwardedHeaders.KnownNetworks)
                {
                    if (!System.Net.IPNetwork.TryParse(network, out var parsed))
                        throw new InvalidOperationException($"Security:ForwardedHeaders:KnownNetworks contains '{network}', which is not a CIDR range such as 172.16.0.0/12.");
                    options.KnownIPNetworks.Add(parsed);
                }
            });
        }

        return services;
    }

    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("AppContext");

        services.AddDbContext<ApplicationContext>(options =>
                {
                    options.UseLazyLoadingProxies()
                        .UseSqlServer(connectionString, sqlOptions =>
                            sqlOptions.EnableRetryOnFailure(
                                maxRetryCount: 5,
                                maxRetryDelay: TimeSpan.FromSeconds(10),
                                errorNumbersToAdd: null));

                    if (environment.IsDevelopment())
                    {
                        options.EnableDetailedErrors()
                            .EnableSensitiveDataLogging()
                            .UseLoggerFactory(LoggerFactory.Create(b => b.AddDebug()));
                    }
                });

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        return services;
    }

    public static IServiceCollection AddAppUrl(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IAppUrlProvider>(new AppUrlProvider(configuration["App:BaseUrl"]));

        var version = configuration["App:Version"];
        if (string.IsNullOrWhiteSpace(version))
            version = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3);
        services.AddSingleton<IAppInfoProvider>(new AppInfoProvider(
            version,
            configuration.GetValue<bool>("App:IsBeta"),
            configuration["App:BetaMessage"]));

        return services;
    }

    public static IServiceCollection AddApplicationSettings(this IServiceCollection services)
    {
        var settingTypes = new List<Type>();
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                settingTypes.AddRange(assembly.GetTypes()
                    .Where(x => typeof(ISettings).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract));
            }
            catch
            {

            }
        }

        foreach (var settingType in settingTypes)
        {
            services.AddScoped(settingType, serviceProvider =>
                serviceProvider.GetRequiredService<ISettingService>().LoadSettingAsync(settingType).Result);
        }

        return services;
    }

    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddRouting(options => options.LowercaseUrls = true);

        services.AddControllersWithViews(options =>
                {
                    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
                }).AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                });

        services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddMvcCore(options =>
                {
                    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
                    options.ModelMetadataDetailsProviders.Add(new MetadataProvider());
                }).AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });

        services.AddSignalR();
        services.AddSingleton<IRealtimeAlertNotifier, RealtimeAlertNotifier>();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services.AddRateLimiter(options =>
        {
            options.AddPolicy(ForgotPasswordRateLimitPolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(5),
                        QueueLimit = 0
                    }));

            options.AddPolicy(GitWebhookRateLimitPolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 300,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            options.OnRejected = (context, _) =>
            {
                if (context.HttpContext.Request.Path.StartsWithSegments("/api"))
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    return ValueTask.CompletedTask;
                }

                context.HttpContext.Response.Redirect("/ForgotPassword?throttled=1");
                return ValueTask.CompletedTask;
            };
        });

        return services;
    }

    public const string ForgotPasswordRateLimitPolicy = "ForgotPassword";

    public const string GitWebhookRateLimitPolicy = "GitWebhook";

    public static IServiceCollection AddAppAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(a =>
        {
            a.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            a.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        }).AddCookie(options =>
        {
            options.AccessDeniedPath = "/account/forbidden/";
            options.LoginPath = "/Login";
            options.ExpireTimeSpan = TimeSpan.FromDays(30);
            options.SlidingExpiration = true;
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Events = new CookieAuthenticationEvents
            {
                OnValidatePrincipal = ValidatePrincipalAsync
            };
        });

        services.AddSingleton<IPostConfigureOptions<CookieAuthenticationOptions>, CookieSessionTimeoutSetup>();

        return services;
    }

    private static readonly TimeSpan RevalidationInterval = TimeSpan.FromMinutes(15);

    private static async Task ValidatePrincipalAsync(CookieValidatePrincipalContext context)
    {
        var principal = context.Principal;
        if (principal?.Identity?.IsAuthenticated != true)
            return;

        var issuedUtc = context.Properties.IssuedUtc;
        if (issuedUtc != null && DateTimeOffset.UtcNow - issuedUtc.Value < RevalidationInterval)
            return;

        var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
        if (idClaim == null || !int.TryParse(idClaim.Value, out var userId))
        {
            context.RejectPrincipal();
            return;
        }

        var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
        var user = await userService.GetByIdAsync(userId);

        if (user == null || user.Deleted || user.Locked || user.Status != (int)UserStatus.Active)
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return;
        }

        var currentRoles = (await userService.GetUserRolesAsync(user))
                    .Select(role => role.SystemName).OrderBy(name => name).ToList();
        var claimRoles = principal.FindAll(ClaimTypes.Role)
            .Select(claim => claim.Value).OrderBy(name => name).ToList();

        if (!currentRoles.SequenceEqual(claimRoles))
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return;
        }

        context.ShouldRenew = true;
    }

    public static IServiceCollection AddProfiling(this IServiceCollection services)
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

        return services;
    }

    public static IServiceCollection AddBackgroundJobs(this IServiceCollection services, IConfiguration configuration, bool startServer = true)
    {
        var connectionString = configuration.GetConnectionString("AppContext");

        services.AddSingleton<HangfireAuthorizationFilter>();

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

        if (startServer)
        {
            services.AddHangfireServer(options =>
                    {
                        options.WorkerCount = 1;
                        options.Queues = ["default"];
                    });
        }

        return services;
    }

    private static void EnsureWritableDirectory(string path)
    {
        try
        {
            Directory.CreateDirectory(path);
            var probe = Path.Combine(path, $".write-probe-{Guid.NewGuid():N}");
            File.WriteAllText(probe, string.Empty);
            File.Delete(probe);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new InvalidOperationException(
                $"The Data Protection key folder '{path}' is not writable. Taskist stores its encryption keys there; without them every sign-in and stored secret is lost on restart. Grant write access or set DataProtection:KeysPath.", ex);
        }
    }
}
