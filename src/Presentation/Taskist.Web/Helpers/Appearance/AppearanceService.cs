using Taskist.Core.Common;
using Taskist.Core.Domain.Users;
using Taskist.Service.Masters;

namespace Taskist.Web.Helpers.Appearance;

public class AppearanceService : IAppearanceService
{
    #region Fields

    protected readonly IGenericAttributeService _genericAttributeService;
    protected readonly IHttpContextAccessor _httpContextAccessor;
    protected readonly IWorkContext _workContext;
    private AppearanceSettings? _cached;

    #endregion

    #region Ctor

    public AppearanceService(IGenericAttributeService genericAttributeService,
        IHttpContextAccessor httpContextAccessor,
        IWorkContext workContext)
    {
        _genericAttributeService = genericAttributeService;
        _httpContextAccessor = httpContextAccessor;
        _workContext = workContext;
    }

    #endregion

    #region Methods

    public async Task<AppearanceSettings> ResolveAsync()
    {
        if (_cached != null)
            return _cached;

        var cookie = _httpContextAccessor.HttpContext?.Request.Cookies[AppearanceOptions.CookieName];
        if (TryParse(cookie, out var fromCookie))
            return _cached = fromCookie;

        var user = await _workContext.GetCurrentUserAsync();
        if (user == null)
            return _cached = new AppearanceSettings();

        var settings = await GetForUserAsync(user);
        WriteCookie(settings);
        return _cached = settings;
    }

    public async Task<AppearanceSettings> GetForUserAsync(User user)
    {
        return new AppearanceSettings
        {
            Mode = AppearanceOptions.NormalizeMode(await _genericAttributeService.GetAttributeAsync<string>(user, AppearanceOptions.ModeAttribute)),
            Accent = AppearanceOptions.NormalizeAccent(await _genericAttributeService.GetAttributeAsync<string>(user, AppearanceOptions.AccentAttribute))
        };
    }

    public async Task<AppearanceSettings> SaveAsync(User user, string? mode, string? accent)
    {
        var current = await GetForUserAsync(user);
        var settings = new AppearanceSettings
        {
            Mode = mode == null ? current.Mode : AppearanceOptions.NormalizeMode(mode),
            Accent = accent == null ? current.Accent : AppearanceOptions.NormalizeAccent(accent)
        };

        await _genericAttributeService.SaveAttributeAsync(user, AppearanceOptions.ModeAttribute, settings.Mode);
        await _genericAttributeService.SaveAttributeAsync(user, AppearanceOptions.AccentAttribute, settings.Accent);
        WriteCookie(settings);

        return _cached = settings;
    }

    public async Task SyncCookieAsync(User user)
    {
        WriteCookie(_cached = await GetForUserAsync(user));
    }

    #endregion

    #region Utilities

    private static bool TryParse(string? value, out AppearanceSettings settings)
    {
        settings = new AppearanceSettings();
        var parts = (value ?? string.Empty).Split('.');
        if (parts.Length != 2)
            return false;

        settings.Mode = AppearanceOptions.NormalizeMode(parts[0]);
        settings.Accent = AppearanceOptions.NormalizeAccent(parts[1]);
        return true;
    }

    private void WriteCookie(AppearanceSettings settings)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null || context.Response.HasStarted)
            return;

        context.Response.Cookies.Append(AppearanceOptions.CookieName, $"{settings.Mode}.{settings.Accent}", new CookieOptions
        {
            HttpOnly = true,
            Secure = context.Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            IsEssential = true,
            Expires = DateTimeOffset.UtcNow.AddYears(1)
        });
    }

    #endregion
}
