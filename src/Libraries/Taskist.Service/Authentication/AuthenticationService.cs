using Taskist.Core.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Taskist.Core.Caching;
using Taskist.Core.Domain.Settings;
using Taskist.Core.Domain.Users;
using Taskist.Service.Common;
using Taskist.Service.Masters;
using Taskist.Service.Users;

namespace Taskist.Service.Authentication;

public class AuthenticationService : IAuthenticationService
{
    #region Fields

    protected readonly IUserService _userService;
    protected readonly IHttpContextAccessor _httpContextAccessor;
    protected readonly ICacheManager _cacheManager;
    protected readonly ISettingService _settingService;

    private User _cachedUser;

    #endregion

    #region Ctor

    public AuthenticationService(IUserService userService,
            IHttpContextAccessor httpContextAccessor,
            ICacheManager cacheManager,
            ISettingService settingService)
    {
        _userService = userService;
        _httpContextAccessor = httpContextAccessor;
        _cacheManager = cacheManager;
        _settingService = settingService;
    }

    #endregion

    #region Methods

    public async Task SignInAsync(User entity, bool isPersistent)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var claims = new List<Claim>();

        if (!string.IsNullOrEmpty(entity.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, entity.Email, ClaimValueTypes.String, ServiceConstant.ClaimsIssuer));
            claims.Add(new Claim(ClaimTypes.Name, entity.Name, ClaimValueTypes.String, ServiceConstant.ClaimsIssuer));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, entity.Id.ToString(), ClaimValueTypes.Integer, ServiceConstant.ClaimsIssuer));
            claims.Add(new Claim(ServiceConstant.LanguageIdClaim, entity.LanguageId.ToString(), ClaimValueTypes.Integer, ServiceConstant.ClaimsIssuer));

            var roles = await _userService.GetUserRolesAsync(entity);
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role.SystemName, ClaimValueTypes.String, ServiceConstant.ClaimsIssuer));

            var isAdmin = roles.Any(r => r.SystemName == UserConstant.AdministratorsRoleName);
            claims.Add(new Claim(ServiceConstant.IsAdminClaim, isAdmin.ToString(), ClaimValueTypes.Boolean, ServiceConstant.ClaimsIssuer));
        }

        var userIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var userPrincipal = new ClaimsPrincipal(userIdentity);

        var security = await _settingService.LoadSettingAsync<SecuritySettings>();
        var idleMinutes = security.SessionTimeoutMinutes > 0 ? security.SessionTimeoutMinutes : 180;
        var rememberDays = security.RememberMeDurationDays > 0 ? security.RememberMeDurationDays : 30;

        var authenticationProperties = new AuthenticationProperties
        {
            IsPersistent = isPersistent,
            AllowRefresh = true,
            IssuedUtc = DateTimeOffset.UtcNow,
            ExpiresUtc = isPersistent
                ? DateTimeOffset.UtcNow.AddDays(rememberDays)
                : DateTimeOffset.UtcNow.AddMinutes(idleMinutes)
        };

        await _httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    userPrincipal, authenticationProperties);

        _cachedUser = entity;
    }

    public async Task SignOutAsync()
    {
        _cachedUser = null;
        await _cacheManager.ClearAsync();
        await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    public async Task<User> GetAuthenticatedUserAsync()
    {
        if (_cachedUser != null)
            return _cachedUser;

        var authenticateResult = await _httpContextAccessor.HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!authenticateResult.Succeeded)
            return null;

        User user = null;
        var claims = authenticateResult.Principal.FindFirst(claim => claim.Type == ClaimTypes.Email
                && claim.Issuer.Equals(ServiceConstant.ClaimsIssuer, StringComparison.InvariantCultureIgnoreCase));
        if (claims != null)
            user = await _userService.GetByEmailAsync(claims.Value);

        if (user == null ||
                    user.Status != (int)UserStatus.Active || user.Deleted)
            return null;

        var isAdminClaim = authenticateResult.Principal.FindFirst(claim => claim.Type == ServiceConstant.IsAdminClaim
                        && claim.Issuer.Equals(ServiceConstant.ClaimsIssuer, StringComparison.InvariantCultureIgnoreCase));
        user.IsAdmin = isAdminClaim != null
            ? bool.TryParse(isAdminClaim.Value, out var admin) && admin
            : await _userService.IsAdminAsync(user);

        _cachedUser = user;

        return _cachedUser;
    }

    #endregion
}