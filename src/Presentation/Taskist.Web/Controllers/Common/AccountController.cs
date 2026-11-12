using System.Globalization;
using System.Security.Claims;
using AuthenticationProperties = Microsoft.AspNetCore.Authentication.AuthenticationProperties;
using AuthenticationHttpContextExtensions = Microsoft.AspNetCore.Authentication.AuthenticationHttpContextExtensions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Taskist.Core.Common;
using Taskist.Core.Domain.Users;
using Taskist.Core.Enums;
using Taskist.Service.Authentication;
using Taskist.Service.Localization;
using Taskist.Service.Logging;
using Taskist.Service.Masters;
using Taskist.Service.Messages;
using Taskist.Service.Security;
using Taskist.Service.Users;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Users;

using Taskist.Web.Helpers.Appearance;
using Taskist.Web.Helpers.Sso;
using Taskist.Web.Helpers.TwoFactor;
using Taskist.Web.Infrastructure;
using Taskist.Core.Domain.Settings;

namespace Taskist.Web.Controllers.Common;

public class AccountController : Controller
{
    #region Fields

    protected readonly IUserService _userService;
    protected readonly ISystemService _systemService;
    protected readonly IGenericAttributeService _genericAttributeService;
    protected readonly IAuthenticationService _authenticationService;
    protected readonly IEncryptionService _encryptionService;
    protected readonly IMessageService _messageService;
    protected readonly IUserActivityService _activityService;
    protected readonly IAppearanceService _appearanceService;
    protected readonly ILocalizationService _localizationService;
    protected readonly ISsoSignInService _ssoSignInService;
    protected readonly OidcOptions _oidcOptions;
    protected readonly ILogger<AccountController> _logger;
    protected readonly ITwoFactorService _twoFactorService;
    protected readonly TwoFactorOptions _twoFactorOptions;
    protected readonly ISettingService _settingService;
    protected readonly IDataProtector _deviceProtector;

    private const string PendingUserClaim = "taskist:2fa:uid";
    private const string PendingRememberClaim = "taskist:2fa:remember";
    private const string PendingReturnUrlClaim = "taskist:2fa:return";
    private const string PendingAttemptsClaim = "taskist:2fa:attempts";
    private const string TwoFactorExpiredCode = "expired";

    #endregion

    #region Ctor

    public AccountController(IUserService userService,
        ISystemService systemService,
        IGenericAttributeService genericAttributeService,
        IAuthenticationService authenticationService,
        IEncryptionService encryptionService,
        IMessageService messageService,
        IUserActivityService activityService,
        IAppearanceService appearanceService,
        ILocalizationService localizationService,
        ISsoSignInService ssoSignInService,
        OidcOptions oidcOptions,
        ILogger<AccountController> logger,
        ITwoFactorService twoFactorService,
        TwoFactorOptions twoFactorOptions,
        ISettingService settingService,
        IDataProtectionProvider dataProtectionProvider)
    {
        _logger = logger;
        _twoFactorService = twoFactorService;
        _twoFactorOptions = twoFactorOptions;
        _settingService = settingService;
        _deviceProtector = dataProtectionProvider.CreateProtector(RememberedDeviceToken.Purpose);
        _appearanceService = appearanceService;
        _localizationService = localizationService;
        _ssoSignInService = ssoSignInService;
        _oidcOptions = oidcOptions;
        _userService = userService;
        _systemService = systemService;
        _genericAttributeService = genericAttributeService;
        _authenticationService = authenticationService;
        _encryptionService = encryptionService;
        _messageService = messageService;
        _activityService = activityService;
    }

    #endregion

    #region Actions

    public IActionResult Login(string sso, string local, string tf)
    {
        var model = new LoginModel { LocalLogin = LocalLoginSwitch.IsOn(local) };
        PrepareLoginModel(model);
        model.SsoErrorCode = SsoErrorCode.Normalize(sso);
        model.TwoFactorExpired = string.Equals(tf, TwoFactorExpiredCode, StringComparison.Ordinal);

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginModel model, string returnUrl)
    {
        if (ModelState.IsValid)
        {
            var loginResult = await _userService.ValidateAsync(model.Email, model.Password);

            switch (loginResult)
            {
                case LoginResult.Successful:
                    {
                        var user = await _userService.GetByEmailAsync(model.Email);
                        if (_oidcOptions.LocalLoginRestricted && !await IsAdministratorAsync(user))
                        {
                            ModelState.AddModelError(string.Empty, await ResourceOrDefaultAsync("Login.Sso.PasswordSignInDisabled",
                                $"Password sign-in is turned off. Use \"Sign in with {_oidcOptions.EffectiveDisplayName}\" instead."));
                            await _activityService.InsertAsync(user, ActivityType.LoginFailed, "Failed sign-in: password sign-in is limited to administrators while single sign-on is required");
                            PrepareLoginModel(model);
                            return View(model);
                        }

                        return await ContinueAfterPasswordAsync(user, model.RememberMe, returnUrl);
                    }
                case LoginResult.NotExist:
                    ModelState.AddModelError(string.Empty, "User doesn't exists");
                    break;
                case LoginResult.NotActive:
                    ModelState.AddModelError(string.Empty, "User is not active");
                    break;
                case LoginResult.Deleted:
                    ModelState.AddModelError(string.Empty, "No account with this email found");
                    break;
                case LoginResult.NotRegistered:
                    ModelState.AddModelError(string.Empty, "Account is not registered");
                    break;
                case LoginResult.LockedOut:
                    ModelState.AddModelError(string.Empty, "Your account is locked out");
                    break;
                case LoginResult.WrongPassword:
                default:
                    ModelState.AddModelError(string.Empty, "The credentials provided are incorrect");
                    break;
            }

            await _activityService.InsertAsync(await _userService.GetByEmailAsync(model.Email),
                ActivityType.LoginFailed, $"Failed sign-in: {loginResult}");
        }

        PrepareLoginModel(model);
        return View(model);
    }

    [HttpPost]
    public IActionResult ExternalLogin(string returnUrl)
    {
        if (!_oidcOptions.IsActive)
            return NotFound();

        var callback = Url.Action(nameof(ExternalCallback), "Account", new { returnUrl = SafeReturnUrl(returnUrl) });
        var properties = new AuthenticationProperties { RedirectUri = callback };

        return Challenge(properties, OidcOptions.SchemeName);
    }

    public async Task<IActionResult> ExternalCallback(string returnUrl)
    {
        if (!_oidcOptions.IsActive)
            return NotFound();

        var external = await AuthenticationHttpContextExtensions.AuthenticateAsync(HttpContext, OidcOptions.ExternalCookieScheme);
        await AuthenticationHttpContextExtensions.SignOutAsync(HttpContext, OidcOptions.ExternalCookieScheme);

        if (!external.Succeeded || external.Principal == null)
            return RedirectToAction(nameof(Login), new { sso = SsoErrorCode.Failed });

        var identity = ExternalIdentityReader.Read(external.Principal.Claims, _oidcOptions.EffectiveRoleClaim);
        var result = await _ssoSignInService.ResolveAsync(identity);

        if (!result.Succeeded)
        {
            _logger.LogWarning("Single sign-on refused ({Reason}) for an identity from {Issuer}.", result.ErrorCode, identity.Issuer);
            return RedirectToAction(nameof(Login), new { sso = result.ErrorCode });
        }

        var note = result.Provisioned ? "Signed in with single sign-on (account created)"
            : result.Linked ? "Signed in with single sign-on (account linked)"
            : "Signed in with single sign-on";
        if (result.RolesAdded.Count > 0)
            note += $"; roles added: {string.Join(", ", result.RolesAdded)}";

        return await CompleteSignInAsync(result.User!, false, note, returnUrl, AuthMethod.SingleSignOn);
    }

    public async Task<IActionResult> TwoFactor()
    {
        var pending = await AuthenticationHttpContextExtensions.AuthenticateAsync(HttpContext, TwoFactorOptions.PendingScheme);
        if (!pending.Succeeded || pending.Principal == null)
            return RedirectToAction(nameof(Login), new { tf = TwoFactorExpiredCode });

        return View(new TwoFactorLoginModel { RememberDeviceDays = _twoFactorOptions.EffectiveRememberDeviceDays });
    }

    [HttpPost]
    public async Task<IActionResult> TwoFactor(TwoFactorLoginModel model)
    {
        var pending = await AuthenticationHttpContextExtensions.AuthenticateAsync(HttpContext, TwoFactorOptions.PendingScheme);
        var principal = pending.Principal;
        if (!pending.Succeeded || principal == null
            || !int.TryParse(principal.FindFirst(PendingUserClaim)?.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var userId))
            return RedirectToAction(nameof(Login), new { tf = TwoFactorExpiredCode });

        var user = await _userService.GetByIdAsync(userId);
        if (user == null || user.Deleted || user.Locked || user.Status != (int)UserStatus.Active)
            return await AbandonTwoFactorAsync();

        var rememberMe = principal.FindFirst(PendingRememberClaim)?.Value == bool.TrueString;
        var returnUrl = principal.FindFirst(PendingReturnUrlClaim)?.Value;
        model.RememberDeviceDays = _twoFactorOptions.EffectiveRememberDeviceDays;

        var verified = model.IsRecoveryMode
            ? await _twoFactorService.UseRecoveryCodeAsync(user.Id, model.Code)
            : await _twoFactorService.VerifyCodeAsync(user.Id, model.Code);

        if (!verified)
        {
            var attempts = (int.TryParse(principal.FindFirst(PendingAttemptsClaim)?.Value, out var count) ? count : 0) + 1;
            var locked = await RegisterFailedAttemptAsync(user);
            await _activityService.InsertAsync(user, ActivityType.LoginFailed,
                model.IsRecoveryMode ? "Failed sign-in: wrong recovery code" : "Failed sign-in: wrong two-factor code");

            if (locked || attempts >= TwoFactorOptions.MaxAttempts)
                return await AbandonTwoFactorAsync();

            await SignInPendingAsync(user.Id, rememberMe, returnUrl, attempts);
            model.Code = null;
            model.ErrorMessage = await ResourceOrDefaultAsync(model.IsRecoveryMode ? "Login.TwoFactor.WrongRecoveryCode" : "Login.TwoFactor.WrongCode",
                model.IsRecoveryMode ? "That recovery code isn't valid or has already been used." : "That code isn't right. Check your authenticator app and try again.");
            return View(model);
        }

        await AuthenticationHttpContextExtensions.SignOutAsync(HttpContext, TwoFactorOptions.PendingScheme);
        await ResetFailedAttemptsAsync(user);

        if (model.RememberDevice == true && _twoFactorOptions.CanRememberDevice)
            await RememberDeviceAsync(user.Id);

        var note = model.IsRecoveryMode
            ? $"Signed in with a recovery code ({await _twoFactorService.CountRemainingRecoveryCodesAsync(user.Id)} left)"
            : "Signed in with a two-factor code";

        return await CompleteSignInAsync(user, rememberMe, note, returnUrl, AuthMethod.LocalWithCode);
    }

    public async Task<IActionResult> Logout()
    {
        await _activityService.InsertAsync(ActivityType.Logout, "Signed out");

        await HttpContext.Session.ClearAsync();
        await _authenticationService.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    #endregion

    #region Forgot Password

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordModel());
    }

    [HttpPost]
    [EnableRateLimiting("ForgotPassword")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userService.GetByEmailAsync(model.Email);
        if (user != null && user.Id > 0 && !user.Deleted && user.Status == (int)UserStatus.Active)
        {
            var token = _encryptionService.GenerateToken(user.Code);
            await _messageService.EmailPasswordResetLinkAsync(user, token);
        }

        TempData["ForgotPasswordSent"] = true;
        return RedirectToRoute("ForgotPassword");
    }

    #endregion

    #region Activate Account

    public async Task<IActionResult> Activate(string token)
    {
        var message = "It seems like you have clicked an invalid or expired link, please contact your administrator.";
        var decodedToken = token;
        var model = new SetPasswordModel
        {
            Token = decodedToken
        };

        if (string.IsNullOrEmpty(decodedToken))
        {
            model.Valid = false;
            model.Message = message;
            return View(model);
        }

        var user = await _userService.ValidateTokenAsync(decodedToken);
        if (user != null && user.Id > 0)
        {
            model.Name = user.Name;
            model.Email = user.Email;
            model.Valid = true;
            model.Code = user.Code;
            model.Password = null;
        }
        else
        {
            model.Valid = false;
            model.Message = message;
        }
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Activate(SetPasswordModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userService.ValidateTokenAsync(model.Token);
            if (user != null && user.Id > 0 && user.Code == model.Code && user.Email == model.Email)
            {
                await _userService.ResetPasswordAsync(user.Id, model.Password);
                return RedirectToRoute("Login");
            }
            else
            {
                return RedirectToRoute("Activate");
            }
        }

        return View(model);
    }

    #endregion

    #region Utilities

    private void PrepareLoginModel(LoginModel model)
    {
        model.SsoEnabled = _oidcOptions.IsActive;
        model.SsoDisplayName = _oidcOptions.EffectiveDisplayName;
        model.ShowPasswordForm = !_oidcOptions.LocalLoginRestricted || model.LocalLogin;
    }

    private async Task<bool> IsAdministratorAsync(User user)
    {
        if (user == null)
            return false;

        return await _userService.IsInUserRoleAsync(user, UserConstant.SystemAdministratorRoleName)
            || await _userService.IsAdminAsync(user);
    }

    private async Task<string> ResourceOrDefaultAsync(string key, string fallback)
    {
        var value = await _localizationService.GetResourceAsync(key);
        return string.IsNullOrWhiteSpace(value) || string.Equals(value, key, StringComparison.OrdinalIgnoreCase) ? fallback : value;
    }

    private string SafeReturnUrl(string returnUrl) =>
        !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : null;

    private async Task<IActionResult> ContinueAfterPasswordAsync(User user, bool rememberMe, string returnUrl)
    {
        var twoFactor = await _twoFactorService.GetAsync(user.Id);
        if (twoFactor == null)
            return await CompleteSignInAsync(user, rememberMe, "Signed in", returnUrl, AuthMethod.Local);

        if (_twoFactorOptions.CanRememberDevice
            && RememberedDeviceToken.IsValid(_deviceProtector, Request.Cookies[TwoFactorOptions.RememberDeviceCookie], user.Id, twoFactor.EnabledOn.Ticks, DateTimeOffset.UtcNow))
        {
            await ResetFailedAttemptsAsync(user);
            return await CompleteSignInAsync(user, rememberMe, "Signed in (remembered device)", returnUrl, AuthMethod.LocalRememberedDevice);
        }

        await SignInPendingAsync(user.Id, rememberMe, SafeReturnUrl(returnUrl), 0);
        return RedirectToAction(nameof(TwoFactor));
    }

    private async Task SignInPendingAsync(int userId, bool rememberMe, string returnUrl, int attempts)
    {
        var claims = new List<Claim>
        {
            new(PendingUserClaim, userId.ToString(CultureInfo.InvariantCulture)),
            new(PendingRememberClaim, rememberMe ? bool.TrueString : bool.FalseString),
            new(PendingAttemptsClaim, attempts.ToString(CultureInfo.InvariantCulture))
        };
        if (!string.IsNullOrEmpty(returnUrl))
            claims.Add(new Claim(PendingReturnUrlClaim, returnUrl));

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, TwoFactorOptions.PendingScheme));
        await AuthenticationHttpContextExtensions.SignInAsync(HttpContext, TwoFactorOptions.PendingScheme, principal,
            new AuthenticationProperties { IsPersistent = false, ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(5) });
    }

    private async Task<IActionResult> AbandonTwoFactorAsync()
    {
        await AuthenticationHttpContextExtensions.SignOutAsync(HttpContext, TwoFactorOptions.PendingScheme);
        return RedirectToAction(nameof(Login), new { tf = TwoFactorExpiredCode });
    }

    private async Task<bool> RegisterFailedAttemptAsync(User user)
    {
        var security = await _settingService.LoadSettingAsync<SecuritySettings>();
        if (security.MaxFailedLoginAttempts <= 0)
            return false;

        user.FailedLoginAttempts++;
        if (user.FailedLoginAttempts >= security.MaxFailedLoginAttempts)
            user.Locked = true;
        await _userService.UpdateAsync(user);

        return user.Locked;
    }

    private async Task ResetFailedAttemptsAsync(User user)
    {
        if (user.FailedLoginAttempts == 0)
            return;

        user.FailedLoginAttempts = 0;
        await _userService.UpdateAsync(user);
    }

    private async Task RememberDeviceAsync(int userId)
    {
        var twoFactor = await _twoFactorService.GetAsync(userId);
        if (twoFactor == null)
            return;

        var expires = DateTimeOffset.UtcNow.AddDays(_twoFactorOptions.EffectiveRememberDeviceDays);
        Response.Cookies.Append(TwoFactorOptions.RememberDeviceCookie,
            RememberedDeviceToken.Create(_deviceProtector, userId, twoFactor.EnabledOn.Ticks, expires),
            new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Secure = true,
                Expires = expires
            });
    }

    private async Task<IActionResult> CompleteSignInAsync(User user, bool isPersistent, string note, string returnUrl, string authMethod)
    {
        await _authenticationService.SignInAsync(user, isPersistent, authMethod);
        await _appearanceService.SyncCookieAsync(user);

        await _activityService.InsertAsync(user, ActivityType.Login, note);

        await _systemService.ResetCacheAsync();

        var target = SafeReturnUrl(returnUrl);
        if (target == null)
            return RedirectToAction("Index", "Home");
        return Redirect(target);
    }

    #endregion
}