using Microsoft.AspNetCore.Mvc;
using Taskist.Core.Common;
using Taskist.Service.Authentication;
using Taskist.Service.Masters;
using Taskist.Service.Security;
using Taskist.Service.Users;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Users;

namespace Taskist.Web.Controllers.Common;

public class AccountController : Controller
{
    #region Fields

    protected readonly IUserService _userService;
    protected readonly ISystemService _systemService;
    protected readonly IGenericAttributeService _genericAttributeService;
    protected readonly IAuthenticationService _authenticationService;
    protected readonly IEncryptionService _encryptionService;

    #endregion

    #region Ctor

    public AccountController(IUserService userService,
        ISystemService systemService,
        IGenericAttributeService genericAttributeService,
        IAuthenticationService authenticationService,
        IEncryptionService encryptionService)
    {
        _userService = userService;
        _systemService = systemService;
        _genericAttributeService = genericAttributeService;
        _authenticationService = authenticationService;
        _encryptionService = encryptionService;
    }

    #endregion

    #region Actions

    public async Task<IActionResult> Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginModel model, string returnUrl)
    {
        if (ModelState.IsValid)
        {
            var loginResult = await _userService.ValidateAsync(model.Email, model.Password);

            switch (loginResult)
            {
                case LoginResultEnum.Successful:
                    {
                        var user = await _userService.GetByEmailAsync(model.Email);
                        await _authenticationService.SignInAsync(user, model.RememberMe);

                        var lastAccessedProjectId = await _genericAttributeService.GetAttributeAsync<int>(user, Constant.ActiveProjectSession);
                        if (lastAccessedProjectId <= 0)
                        {
                            var accessableProject = await _userService.GetAllAccessibleProjects(user.Id);
                            if (accessableProject.Any())
                            {
                                var firstProject = accessableProject.FirstOrDefault();
                                if (firstProject != null)
                                    await _genericAttributeService.SaveAttributeAsync(user, Constant.ActiveProjectSession, firstProject.Id);
                            }
                        }
                        await _systemService.ResetCacheAsync();

                        if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                            return RedirectToAction("Index", "Home");
                        return Redirect(returnUrl);
                    }
                case LoginResultEnum.NotExist:
                    ModelState.AddModelError(string.Empty, "User doesn't exists");
                    break;
                case LoginResultEnum.NotActive:
                    ModelState.AddModelError(string.Empty, "User is not active");
                    break;
                case LoginResultEnum.Deleted:
                    ModelState.AddModelError(string.Empty, "No account with this email found");
                    break;
                case LoginResultEnum.NotRegistered:
                    ModelState.AddModelError(string.Empty, "Account is not registered");
                    break;
                case LoginResultEnum.LockedOut:
                    ModelState.AddModelError(string.Empty, "Your account is locked out");
                    break;
                case LoginResultEnum.WrongPassword:
                default:
                    ModelState.AddModelError(string.Empty, "The credentials provided are incorrect");
                    break;
            }
        }

        return View(model);
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.Session.ClearAsync();
        await _authenticationService.SignOutAsync();
        return RedirectToAction("Index", "Home");
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