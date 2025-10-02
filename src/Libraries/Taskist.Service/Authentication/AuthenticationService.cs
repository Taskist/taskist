using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Taskist.Core.Caching;
using Taskist.Core.Common;
using Taskist.Core.Domain.Users;
using Taskist.Service.Common;
using Taskist.Service.Users;

namespace Taskist.Service.Authentication;

public class AuthenticationService : IAuthenticationService
{
    #region Fields

    protected readonly IUserService _userService;
    protected readonly IHttpContextAccessor _httpContextAccessor;
    protected readonly ICacheManager _cacheManager;

    private User _cachedUser;

    #endregion

    #region Ctor

    public AuthenticationService(IUserService userService,
        IHttpContextAccessor httpContextAccessor,
        ICacheManager cacheManager)
    {
        _userService = userService;
        _httpContextAccessor = httpContextAccessor;
        _cacheManager = cacheManager;

    }

    #endregion

    #region Methods

    public async Task SignInAsync(User entity, bool isPersistent)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var claims = new List<Claim>();

        if (!string.IsNullOrEmpty(entity.Email))
            claims.Add(new Claim(ClaimTypes.Email, entity.Email, ClaimValueTypes.String, ServiceConstant.ClaimsIssuer));

        var userIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var userPrincipal = new ClaimsPrincipal(userIdentity);

        var authenticationProperties = new AuthenticationProperties
        {
            IsPersistent = isPersistent,
            IssuedUtc = DateTime.UtcNow
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
            user.Status != (int)UserStatusEnum.Active || user.Deleted)
            return null;

        user.IsAdmin = await _userService.IsAdminAsync(user);

        _cachedUser = user;

        return _cachedUser;
    }

    #endregion
}