using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using Taskist.Core.Domain.Settings;
using Taskist.Service.Masters;

namespace Taskist.Web.Infrastructure;

public class CookieSessionTimeoutSetup : IPostConfigureOptions<CookieAuthenticationOptions>
{
    #region Fields

    private readonly IServiceScopeFactory _scopeFactory;

    #endregion

    #region Ctor

    public CookieSessionTimeoutSetup(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    #endregion

    #region Methods

    public void PostConfigure(string name, CookieAuthenticationOptions options)
    {
        if (name != CookieAuthenticationDefaults.AuthenticationScheme)
            return;

        using var scope = _scopeFactory.CreateScope();
        var settingService = scope.ServiceProvider.GetRequiredService<ISettingService>();
        var security = settingService.LoadSettingAsync<SecuritySettings>().GetAwaiter().GetResult();

        if (security.SessionTimeoutMinutes > 0)
        {
            options.ExpireTimeSpan = TimeSpan.FromMinutes(security.SessionTimeoutMinutes);
            options.SlidingExpiration = true;
        }
    }

    #endregion
}
