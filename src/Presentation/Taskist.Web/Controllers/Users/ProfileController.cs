using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using AutoMapper;
using Taskist.Core.Common;
using Taskist.Service.Security;
using Taskist.Service.Localization;
using Taskist.Service.Logging;
using Taskist.Service.Users;
using Taskist.Service.Files;
using Taskist.Web.Controllers.Common;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Helpers.Common;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Common;
using Taskist.Web.Models.Users;

namespace Taskist.Web.Controllers.Users;

public class ProfileController : BaseController
{
    #region Fields

    protected readonly IUserService _userService;
    protected readonly IFileStorageService _fileStorageService;
    protected readonly ILanguageService _languageService;
    protected readonly IPermissionService _permissionService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IUserActivityService _userActivityService;
    protected readonly IWorkContext _workContext;
    protected readonly IMapper _mapper;

    #endregion

    #region Ctor

    public ProfileController(IUserService userService,
        IFileStorageService fileStorageService,
        ILanguageService languageService,
        IPermissionService permissionService,
        ILocalizationService localizationService,
        IUserActivityService userActivityService,
        IWorkContext workContext,
        IMapper mapper)
    {
        _userService = userService;
        _fileStorageService = fileStorageService;
        _languageService = languageService;
        _permissionService = permissionService;
        _localizationService = localizationService;
        _userActivityService = userActivityService;
        _workContext = workContext;
        _mapper = mapper;
    }

    #endregion

    #region Actions

    [CheckPermission(PermissionProvider.General.MANAGE_DASHBOARD)]
    public async Task<IActionResult> Edit()
    {
        var user = await _workContext.GetCurrentUserAsync();
        var model = new ProfileModel
        {
            Detail = _mapper.Map<ProfileDetailModel>(user),
            AvatarVersion = user.AvatarVersion,
            DisplayName = user.Name
        };

        await InitModelAsync(model.Detail);

        return PartialView(model);
    }

    [HttpPost]
    [CheckPermission(PermissionProvider.General.MANAGE_DASHBOARD)]
    public async Task<IActionResult> Edit(ProfileDetailModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = await _userService.GetByIdAsync(model.Id);
            entity = _mapper.Map(model, entity);

            await _userService.UpdateAsync(entity);

            await _userActivityService.InsertAsync("Profile", string.Format(await _localizationService.GetResourceAsync("Log.RecordUpdated"), entity.Name), entity);

            return Json(new JsonResponseModel
            {
                Status = HttpStatusCodeEnum.Success,
                Message = await _localizationService.GetResourceAsync("Message.UpdateSuccess")
            });
        }

        return Json(new JsonResponseModel
        {
            Status = ModelState.IsValid ? HttpStatusCodeEnum.InternalServerError : HttpStatusCodeEnum.ValidationError,
            Message = await _localizationService.GetResourceAsync("Error.Failed"),
            Errors = ModelState.AllErrors()
        });
    }

    [HttpPost]
    [CheckPermission(PermissionProvider.General.MANAGE_DASHBOARD)]
    public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = await _workContext.GetCurrentUserAsync();
            await _userService.ResetPasswordAsync(entity.Id, model.NewPassword);

            await _userActivityService.InsertAsync("ChangePassword", string.Format(await _localizationService.GetResourceAsync("Log.RecordUpdated"), entity.Name), entity);

            return Json(new JsonResponseModel
            {
                Status = HttpStatusCodeEnum.Success,
                Message = await _localizationService.GetResourceAsync("Message.UpdateSuccess")
            });
        }

        return Json(new JsonResponseModel
        {
            Status = ModelState.IsValid ? HttpStatusCodeEnum.InternalServerError : HttpStatusCodeEnum.ValidationError,
            Message = await _localizationService.GetResourceAsync("Error.Failed"),
            Errors = ModelState.AllErrors()
        });
    }

    [HttpGet]
    // Client-side cache only; the browser already keys its cache by the full URL (incl. ?userId),
    // so VaryByQueryKeys (which needs the response-cache middleware) is neither required nor wanted.
    [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Client)]
    [CheckPermission(PermissionProvider.General.MANAGE_DASHBOARD)]
    public async Task<IActionResult> GetAvatar(int userId = 0)
    {
        //userId lets other views (history / comments timelines) show each author's photo;
        //without it the current user's own avatar is served
        var user = userId > 0
            ? await _userService.GetByIdAsync(userId)
            : await _workContext.GetCurrentUserAsync();

        user ??= await _workContext.GetCurrentUserAsync();

        var file = await _fileStorageService.GetAvatarFileAsync(user);

        return File(file.FileBytes, "image/png");
    }

    [HttpPost]
    [CheckPermission(PermissionProvider.General.MANAGE_DASHBOARD)]
    public async Task<IActionResult> UploadAvatar(IFormFile avatar)
    {
        if (avatar == null || avatar.Length == 0)
            return BadRequest("Avatar is required.");

        if (avatar.Length > WebConstant.MaxAvatarBytes)
            return BadRequest("The image must be 2 MB or smaller.");

        //the file is stored with a .png name and served back as an image, so
        //confirm the bytes really are an image before writing it to wwwroot
        if (!ImageHelper.IsImage(avatar))
            return BadRequest("Only PNG, JPEG, GIF or WebP images are accepted.");

        var user = await _workContext.GetCurrentUserAsync();
        if (user == null) return NotFound();

        var fileName = $"{user.Code}.png";

        await _fileStorageService.DeleteFileAsync(fileName, WebConstant.AvatarFolder);
        await _fileStorageService.SaveFileAsync(avatar, WebConstant.AvatarFolder, fileName);
        user.AvatarVersion += 1;
        await _userService.UpdateAsync(user);
        await _workContext.SetCurrentUserAsync(user);

        var version = user.AvatarVersion;
        return Json(new { success = true, version });
    }

    #endregion

    #region Helper

    private async Task InitModelAsync(ProfileDetailModel model)
    {
        var languages = await _languageService.GetAllActiveAsync();

        foreach (var item in languages)
        {
            model.AvailableLanguages.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString(),
                Selected = item.Id == model.LanguageId
            });
        }
    }

    #endregion
}