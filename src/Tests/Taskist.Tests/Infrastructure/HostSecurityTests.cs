using System.Net;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Taskist.Web.Infrastructure;

namespace Taskist.Tests.Infrastructure;

public class HostSecurityTests
{
    #region Utilities

    private static ServiceProvider BuildProvider(Dictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHostSecurity(configuration);
        services.AddAuthentication().AddCookie();
        services.AddSession();
        services.AddAntiforgery();

        return services.BuildServiceProvider();
    }

    private static CookieSecurePolicy AuthCookiePolicy(ServiceProvider provider)
    {
        return provider.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
            .Get(CookieAuthenticationDefaults.AuthenticationScheme).Cookie.SecurePolicy;
    }

    #endregion

    #region Defaults

    [Fact]
    public void Options_DefaultToSecureCookiesAndNoForwardedHeaders()
    {
        var options = new HostSecurityOptions();

        Assert.True(options.RequireHttpsCookies);
        Assert.False(options.ForwardedHeaders.Enabled);
        Assert.Empty(options.ForwardedHeaders.KnownProxies);
        Assert.Empty(options.ForwardedHeaders.KnownNetworks);
    }

    [Fact]
    public void AddHostSecurity_WithoutConfiguration_RequiresHttpsForTheAuthCookie()
    {
        using var provider = BuildProvider([]);

        Assert.Equal(CookieSecurePolicy.Always, AuthCookiePolicy(provider));
        Assert.True(provider.GetRequiredService<HostSecurityOptions>().RequireHttpsCookies);
    }

    #endregion

    #region Cookies

    [Fact]
    public void AddHostSecurity_WhenHttpsCookiesAreOff_AuthCookieFollowsTheRequest()
    {
        using var provider = BuildProvider(new() { ["Security:RequireHttpsCookies"] = "false" });

        Assert.Equal(CookieSecurePolicy.SameAsRequest, AuthCookiePolicy(provider));
    }

    [Theory]
    [InlineData("true")]
    [InlineData("false")]
    public void AddHostSecurity_SessionAndAntiforgeryAlwaysFollowTheRequest(string requireHttps)
    {
        using var provider = BuildProvider(new() { ["Security:RequireHttpsCookies"] = requireHttps });

        Assert.Equal(CookieSecurePolicy.SameAsRequest, provider.GetRequiredService<IOptions<SessionOptions>>().Value.Cookie.SecurePolicy);
        Assert.Equal(CookieSecurePolicy.SameAsRequest, provider.GetRequiredService<IOptions<AntiforgeryOptions>>().Value.Cookie.SecurePolicy);
    }

    #endregion

    #region Forwarded headers

    [Fact]
    public void AddHostSecurity_WhenForwardedHeadersAreOff_LeavesTheMiddlewareUnconfigured()
    {
        using var provider = BuildProvider([]);

        Assert.Equal(ForwardedHeaders.None, provider.GetRequiredService<IOptions<ForwardedHeadersOptions>>().Value.ForwardedHeaders);
    }

    [Fact]
    public void AddHostSecurity_TrustsTheConfiguredProxiesAndNetworks()
    {
        using var provider = BuildProvider(new()
        {
            ["Security:ForwardedHeaders:Enabled"] = "true",
            ["Security:ForwardedHeaders:KnownProxies:0"] = "10.0.0.5",
            ["Security:ForwardedHeaders:KnownNetworks:0"] = "172.16.0.0/12"
        });

        var options = provider.GetRequiredService<IOptions<ForwardedHeadersOptions>>().Value;

        Assert.Equal(ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto, options.ForwardedHeaders);
        Assert.Contains(IPAddress.Parse("10.0.0.5"), options.KnownProxies);
        Assert.Contains(System.Net.IPNetwork.Parse("172.16.0.0/12"), options.KnownIPNetworks);
    }

    [Theory]
    [InlineData("KnownProxies", "not-an-ip")]
    [InlineData("KnownNetworks", "not-a-network")]
    [InlineData("KnownNetworks", "10.0.0.1")]
    public void AddHostSecurity_WithAnInvalidEntry_FailsOnStartup(string key, string value)
    {
        using var provider = BuildProvider(new()
        {
            ["Security:ForwardedHeaders:Enabled"] = "true",
            [$"Security:ForwardedHeaders:{key}:0"] = value
        });

        var error = Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<IOptions<ForwardedHeadersOptions>>().Value);
        Assert.Contains(value, error.Message);
    }

    #endregion

    #region Data Protection key folder

    [Fact]
    public void AddDataProtectionAndSession_WithAWritableFolder_RecordsAnUnprotectedKeyRing()
    {
        var folder = Path.Combine(Path.GetTempPath(), "taskist-keys-" + Guid.NewGuid().ToString("N"));
        try
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["DataProtection:KeysPath"] = folder })
                .Build();
            var services = new ServiceCollection();

            services.AddDataProtectionAndSession(new TestWebHostEnvironment(), configuration);

            using var provider = services.BuildServiceProvider();
            var info = provider.GetRequiredService<Taskist.Core.Security.KeyRingProtectionInfo>();
            Assert.False(info.ProtectedAtRest);
            Assert.Equal(folder, info.KeysPath);
            Assert.True(Directory.Exists(folder));
        }
        finally
        {
            if (Directory.Exists(folder))
                Directory.Delete(folder, true);
        }
    }

    [Fact]
    public void AddDataProtectionAndSession_WhenTheKeyFolderCannotBeCreated_FailsWithAClearMessage()
    {
        var blocker = Path.GetTempFileName();
        try
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["DataProtection:KeysPath"] = blocker })
                .Build();

            var error = Assert.Throws<InvalidOperationException>(() =>
                new ServiceCollection().AddDataProtectionAndSession(new TestWebHostEnvironment(), configuration));

            Assert.Contains("not writable", error.Message);
        }
        finally
        {
            File.Delete(blocker);
        }
    }

    #endregion
}
