using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Taskist.Core.Common;
using Taskist.Core.Enums;
using Taskist.Service.Authentication;
using Taskist.Service.Logging;
using Taskist.Service.Masters;
using Taskist.Service.Messages;
using Taskist.Service.Security;
using Taskist.Service.Users;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Users;

using Taskist.Web.Helpers.Appearance;

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

    #endregion

    #region Ctor

    public AccountController(IUserService userService,
        ISystemService systemService,
        IGenericAttributeService genericAttributeService,
        IAuthenticationService authenticationService,
        IEncryptionService encryptionService,
        IMessageService messageService,
        IUserActivityService activityService,
        IAppearanceService appearanceService)
    {
        _appearanceService = appearanceService;
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

    public async Task<IActionResult> Login()
    {
        return View(new LoginModel());
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
                        await _authenticationService.SignInAsync(user, model.RememberMe);
                        await _appearanceService.SyncCookieAsync(user);

                        await _activityService.InsertAsync(user, ActivityType.Login, "Signed in");

                        await _systemService.ResetCacheAsync();

                        if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                            return RedirectToAction("Index", "Home");
                        return Redirect(returnUrl);
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

        return View(model);
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
}