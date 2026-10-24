using Taskist.Core.Enums;
using Taskist.Core.Common;
using Taskist.Core.Domain.Localization;
using Taskist.Service.Authentication;
using Taskist.Service.Common;
using Taskist.Service.Localization;
using Microsoft.AspNetCore.Localization;
using System.Security.Claims;
using Taskist.Core.Domain.Users;

namespace Taskist.Web.Helpers.Common;

public class WorkContext : IWorkContext
{
    #region Fields

    protected readonly IAuthenticationService _authenticationService;
    protected readonly IHttpContextAccessor _httpContextAccessor;
    protected readonly ILanguageService _languageService;
    protected User _cachedUser;
    protected Language _cachedLanguage;

    #endregion Fields

    #region Ctor

    public WorkContext(IAuthenticationService authenticationService,
                IHttpContextAccessor httpContextAccessor,
                ILanguageService languageService)
    {
        _authenticationService = authenticationService;
        _httpContextAccessor = httpContextAccessor;
        _languageService = languageService;
    }

    #endregion Ctor

    #region Methods

    public async Task<User> GetCurrentUserAsync()
    {
        if (_cachedUser != null)
            return _cachedUser;

        await SetCurrentUserAsync();

        return _cachedUser;
    }

    public int? GetCurrentUserId()
    {
        if (_cachedUser != null)
            return _cachedUser.Id;

        var idClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);

        return idClaim != null && int.TryParse(idClaim.Value, out var id) ? id : null;
    }

    public int? GetCurrentUserLanguageId()
    {
        if (_cachedUser != null)
            return _cachedUser.LanguageId;

        return GetLanguageIdFromClaim();
    }

    public async Task SetCurrentUserAsync(User user = null)
    {
        if (user == null)
            user = await _authenticationService.GetAuthenticatedUserAsync();

        if (user != null && !user.Deleted && user.Status == (int)UserStatus.Active)
        {
            SetUserCookie(user.Code);
            _cachedUser = user;
        }
    }

    public virtual async Task<Language> GetCurrentUserLanguageAsync()
    {
        if (_cachedLanguage != null)
            return _cachedLanguage;

        var currentLanguageId = GetLanguageIdFromClaim() ?? (await GetCurrentUserAsync())?.LanguageId ?? 0;

        var allStoreLanguages = await _languageService.GetAllAsync();

        var detectedLanguage = allStoreLanguages.FirstOrDefault(language => language.Id == currentLanguageId);

        SetLanguageCookie(detectedLanguage);

        _cachedLanguage = detectedLanguage;

        return _cachedLanguage;
    }

    #endregion

    #region  Helper

    protected int? GetLanguageIdFromClaim()
    {
        var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(ServiceConstant.LanguageIdClaim);

        return claim != null && int.TryParse(claim.Value, out var languageId) ? languageId : null;
    }

    protected void SetUserCookie(Guid code)
    {
        if (_httpContextAccessor.HttpContext?.Response?.HasStarted ?? true)
            return;

        var cookieName = WebConstant.UserCookie;
        _httpContextAccessor.HttpContext.Response.Cookies.Delete(cookieName);

        var cookieExpires = WebConstant.UserCookieExpires;
        var cookieExpiresDate = DateTime.Now.AddHours(cookieExpires);

        if (code == Guid.Empty)
            cookieExpiresDate = DateTime.Now.AddMonths(-1);

        var options = new CookieOptions
        {
            HttpOnly = true,
            Expires = cookieExpiresDate,

        };

        _httpContextAccessor.HttpContext.Response.Cookies.Append(cookieName, code.ToString(), options);
    }

    protected virtual void SetLanguageCookie(Language language)
    {
        if (_httpContextAccessor.HttpContext?.Response?.HasStarted ?? true)
            return;

        var cookieName = WebConstant.CultureCookie;
        _httpContextAccessor.HttpContext.Response.Cookies.Delete(cookieName);

        if (string.IsNullOrEmpty(language?.LanguageCulture))
            return;

        var value = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(language.LanguageCulture));
        var options = new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) };
        _httpContextAccessor.HttpContext.Response.Cookies.Append(cookieName, value, options);
    }

    #endregion
}