using Microsoft.AspNetCore.Mvc;
using Taskist.Core.Common;
using Taskist.Core.Domain.Settings;
using Taskist.Core.Enums;
using Taskist.Service.Git;
using Taskist.Service.Localization;
using Taskist.Service.Masters;
using Taskist.Service.Messages;
using Taskist.Service.Security;
using Taskist.Service.Users;
using Taskist.Web.Controllers.Common;
using Taskist.Web.Helpers.Appearance;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Common;
using Taskist.Web.Models.Users;

namespace Taskist.Web.Controllers.Users;

public class ProfileController : BaseController
{
    #region Fields

    protected readonly IUserService _userService;
    protected readonly IDocumentService _documentService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IWorkContext _workContext;
    protected readonly IPermissionService _permissionService;
    private const string OAuthSessionKey = "taskist.git.oauth";

    protected readonly IGitAccountService _gitAccountService;
    protected readonly IGitRepositoryService _gitRepositoryService;
    protected readonly INotificationService _notificationService;
    protected readonly IAppearanceService _appearanceService;

    #endregion

    #region Ctor

    public ProfileController(IUserService userService,
        IDocumentService documentService,
        ILocalizationService localizationService,
        IWorkContext workContext,
        IPermissionService permissionService,
        IGitAccountService gitAccountService,
        IGitRepositoryService gitRepositoryService,
        INotificationService notificationService,
        IAppearanceService appearanceService)
    {
        _appearanceService = appearanceService;
        _gitAccountService = gitAccountService;
        _gitRepositoryService = gitRepositoryService;
        _notificationService = notificationService;
        _userService = userService;
        _documentService = documentService;
        _localizationService = localizationService;
        _workContext = workContext;
        _permissionService = permissionService;
    }

    #endregion

    #region Action

    [CheckPermission(nameof(PermissionProvider.ManageDashboard))]
    public async Task<IActionResult> Index()
    {
        var e = await _userService.GetByIdAsync(UserId);

        var model = new ProfileModel
        {
            FirstName = e.FirstName,
            LastName = e.LastName,
            FriendlyName = e.FriendlyName,
            MobileNumber = e.MobileNumber,
            MaritalStatus = e.MaritalStatus,
            Email = e.Email,
            HasPhoto = e.PhotoDocumentId.HasValue,
            GitConnections = await _gitAccountService.GetStatusesAsync(e),
            Appearance = await _appearanceService.GetForUserAsync(e)
        };

        return View(model);
    }

    [CheckPermission(nameof(PermissionProvider.ManageDashboard))]
    public async Task<IActionResult> GitConnection(int connectionId)
    {
        var e = await _userService.GetByIdAsync(UserId);
        var connection = await _gitRepositoryService.GetConnectionByIdAsync(connectionId);
        if (connection == null || !connection.Active)
            return AccessDeniedPartial();

        return PartialView(new GitConnectionViewModel { Connection = await _gitAccountService.GetStatusAsync(e, connection) });
    }

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageDashboard))]
    public async Task<JsonResponseModel> ConnectGit(int connectionId, string token)
    {
        var e = await _userService.GetByIdAsync(UserId);
        var (success, message) = await _gitAccountService.ConnectWithTokenAsync(e, connectionId, token);

        return new JsonResponseModel
        {
            Status = success ? HttpStatusCode.Success : HttpStatusCode.ValidationError,
            Message = message
        };
    }

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageDashboard))]
    public async Task<JsonResponseModel> DisconnectGit(int connectionId)
    {
        var e = await _userService.GetByIdAsync(UserId);
        await _gitAccountService.DisconnectAsync(e, connectionId);

        return new JsonResponseModel
        {
            Status = HttpStatusCode.Success,
            Message = await _localizationService.GetResourceAsync("Profile.GitLab.Disconnected")
        };
    }

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageDashboard))]
    public async Task<IActionResult> GitOAuthStart(int connectionId)
    {
        var connection = await _gitRepositoryService.GetConnectionByIdAsync(connectionId);
        if (connection == null || !connection.Active)
            return RedirectToAction("Index");

        var provider = _gitRepositoryService.ProviderFor(connection);
        if (!provider.OAuthConfigured)
            return RedirectToAction("Index");

        var state = Base64Url(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        var verifier = Base64Url(System.Security.Cryptography.RandomNumberGenerator.GetBytes(48));
        var challenge = Base64Url(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.ASCII.GetBytes(verifier)));

        HttpContext.Session.SetString(OAuthSessionKey, Newtonsoft.Json.JsonConvert.SerializeObject(new GitOAuthSessionModel
        {
            State = state,
            Verifier = verifier,
            ConnectionId = connection.Id,
            UserId = UserId,
            CreatedOn = DateTime.UtcNow
        }));

        return Redirect(provider.BuildAuthorizeUrl(CallbackUrl(), state, challenge));
    }

    [HttpGet]
    [CheckPermission(nameof(PermissionProvider.ManageDashboard))]
    public async Task<IActionResult> GitOAuthCallback(string? code, string? state, string? error)
    {
        var stored = HttpContext.Session.GetString(OAuthSessionKey);
        HttpContext.Session.Remove(OAuthSessionKey);

        var session = string.IsNullOrEmpty(stored) ? null : Newtonsoft.Json.JsonConvert.DeserializeObject<GitOAuthSessionModel>(stored);
        if (session == null || string.IsNullOrEmpty(state) || session.UserId != UserId
            || session.CreatedOn < DateTime.UtcNow.AddMinutes(-10)
            || !System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(System.Text.Encoding.UTF8.GetBytes(state), System.Text.Encoding.UTF8.GetBytes(session.State)))
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Profile.Git.OAuthInvalid"));
            return RedirectToAction("Index", null, null, "git");
        }

        if (!string.IsNullOrEmpty(error) || string.IsNullOrEmpty(code))
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Profile.Git.OAuthDenied"));
            return RedirectToAction("Index", null, null, "git");
        }

        var connection = await _gitRepositoryService.GetConnectionByIdAsync(session.ConnectionId);
        if (connection == null)
            return RedirectToAction("Index", null, null, "git");

        var exchanged = await _gitRepositoryService.ProviderFor(connection).ExchangeCodeAsync(code, CallbackUrl(), session.Verifier);
        if (!exchanged.Success || exchanged.Data == null)
        {
            _notificationService.ErrorNotification(exchanged.Error ?? await _localizationService.GetResourceAsync("Profile.Git.OAuthDenied"));
            return RedirectToAction("Index", null, null, "git");
        }

        var e = await _userService.GetByIdAsync(UserId);
        var (success, message) = await _gitAccountService.ConnectWithOAuthAsync(e, connection.Id, exchanged.Data);
        if (success)
            _notificationService.SuccessNotification(message);
        else
            _notificationService.ErrorNotification(message);

        return RedirectToAction("Index", null, null, "git");
    }

    [HttpPost]
    public async Task<IActionResult> SaveAppearance(string? mode, string? accent)
    {
        var user = await _userService.GetByIdAsync(UserId);
        if (user == null)
            return Json(new JsonResponseModel { Status = HttpStatusCode.NoData });

        await _appearanceService.SaveAsync(user, mode, accent);

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCode.Success,
            Message = await _localizationService.GetResourceAsync("Profile.Appearance.Saved")
        });
    }

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageDashboard))]
    public async Task<IActionResult> UpdateProfile(ProfileModel model)
    {
        if (!ModelState.IsValid)
            return Json(new JsonResponseModel
            {
                Status = HttpStatusCode.ValidationError,
                Message = await _localizationService.GetResourceAsync("Error.Failed"),
                Errors = ModelState.AllErrors()
            });

        var e = await _userService.GetByIdAsync(UserId);
        e.FirstName = model.FirstName;
        e.LastName = model.LastName;
        e.FriendlyName = model.FriendlyName;
        e.MobileNumber = model.MobileNumber;
        e.MaritalStatus = model.MaritalStatus;
        await _userService.UpdateAsync(e);

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCode.Success,
            Message = await _localizationService.GetResourceAsync("Message.SaveSuccess")
        });
    }

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageDashboard))]
    public async Task<IActionResult> UploadPhoto(IFormFile file)
    {
        if (file == null || !UploadPolicy.IsAllowedImage(file.FileName, file.Length, UploadPolicy.MaxProfilePhotoMegabytes))
            return Json(new JsonResponseModel
            {
                Status = HttpStatusCode.ValidationError,
                Message = await _localizationService.GetResourceAsync("Profile.InvalidImage")
            });

        var doc = await _documentService.InsertAsync(file);

        var e = await _userService.GetByIdAsync(UserId);
        e.PhotoDocumentId = doc.Id;
        await _userService.UpdateAsync(e);

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCode.Success,
            Message = await _localizationService.GetResourceAsync("Message.SaveSuccess")
        });
    }

    [HttpGet]
    [CheckPermission(nameof(PermissionProvider.ManageDashboard))]
    public async Task<IActionResult> Photo(int userId = 0)
    {
        if (userId <= 0)
            userId = UserId;

        var e = await _userService.GetByIdAsync(userId);
        if (e?.PhotoDocumentId != null)
        {
            var doc = await _documentService.GetByIdAsync(e.PhotoDocumentId.Value);
            if (doc != null && UploadPolicy.ImageContentType(doc.Extension) != null)
                return ImageFile(doc);
        }

        return File(System.Text.Encoding.UTF8.GetBytes(InitialsAvatar(e?.FirstName, e?.LastName)), "image/svg+xml");
    }

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageDashboard))]
    public async Task<IActionResult> RemovePhoto()
    {
        var e = await _userService.GetByIdAsync(UserId);
        e.PhotoDocumentId = null;
        await _userService.UpdateAsync(e);

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCode.Success,
            Message = await _localizationService.GetResourceAsync("Message.SaveSuccess")
        });
    }

    private static string InitialsAvatar(string firstName, string lastName)
    {
        var fi = !string.IsNullOrWhiteSpace(firstName) ? char.ToUpper(firstName.Trim()[0]).ToString() : "";
        var li = !string.IsNullOrWhiteSpace(lastName) ? char.ToUpper(lastName.Trim()[0]).ToString() : "";
        var initials = (fi + li);
        if (string.IsNullOrEmpty(initials))
            initials = "?";

        var palette = new[] { "#1cc7c9", "#3399ff", "#2eb85c", "#f9b115", "#e55353", "#9b59b6", "#20a8d8" };
        var name = (firstName ?? "") + (lastName ?? "");
        var hash = 0;
        foreach (var c in name)
            hash = (hash * 31 + c) & 0x7fffffff;
        var bg = palette[hash % palette.Length];

        return $"<svg xmlns='http://www.w3.org/2000/svg' width='128' height='128' viewBox='0 0 128 128'>" +
               $"<rect width='128' height='128' rx='64' fill='{bg}'/>" +
               $"<text x='50%' y='50%' dy='.35em' text-anchor='middle' font-family='system-ui,Segoe UI,Roboto,Arial,sans-serif' font-size='52' font-weight='600' fill='#ffffff'>{initials}</text>" +
               $"</svg>";
    }

    [CheckPermission(nameof(PermissionProvider.ManageDashboard))]
    public async Task<IActionResult> ChangePassword()
    {
        var model = new ChangePasswordModel();

        return PartialView(model);
    }

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageDashboard))]
    public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _workContext.GetCurrentUserAsync();

            await _userService.ResetPasswordAsync(UserId, model.NewPassword);

            return Json(new JsonResponseModel
            {
                Status = HttpStatusCode.Success,
                Message = await _localizationService.GetResourceAsync("Message.SaveSuccess")
            });
        }

        return Json(new JsonResponseModel
        {
            Status = ModelState.IsValid ? HttpStatusCode.InternalServerError : HttpStatusCode.ValidationError,
            Message = await _localizationService.GetResourceAsync("Error.Failed"),
            Errors = ModelState.AllErrors()
        });
    }

    #endregion

    #region Utilities

    private string CallbackUrl() => Url.Action("GitOAuthCallback", "Profile", null, Request.Scheme)!;

    private static string Base64Url(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    #endregion
}
