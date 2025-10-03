using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using Taskist.Core.Common;
using Taskist.Core.Domain.Users;
using Taskist.Service.Localization;
using Taskist.Service.Logging;
using Taskist.Service.Masters;
using Taskist.Service.Security;
using Taskist.Service.Users;
using Taskist.Web.Controllers.Common;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Common;
using Taskist.Web.Models.Datatable;
using Taskist.Web.Models.Users;

namespace Taskist.Web.Controllers.Users;

public class UserController : BaseController
{
    #region Fields

    protected readonly IUserService _userService;
    protected readonly IDocumentService _documentService;
    protected readonly ILanguageService _languageService;
    protected readonly IPermissionService _permissionService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IUserActivityService _userActivityService;
    protected readonly IWorkContext _workContext;
    protected readonly IMapper _mapper;

    #endregion

    #region Ctor

    public UserController(IUserService userService,
        IDocumentService documentService,
        IPermissionService permissionService,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IUserActivityService userActivityService,
        IWorkContext workContext,
        IMapper mapper)
    {
        _userService = userService;
        _documentService = documentService;
        _permissionService = permissionService;
        _languageService = languageService;
        _localizationService = localizationService;
        _userActivityService = userActivityService;
        _workContext = workContext;
        _mapper = mapper;
    }

    #endregion

    #region Action

    [CheckPermission(PermissionProvider.Configuration.MANAGE_USER)]
    public async Task<IActionResult> Index()
    {
        return View();
    }

    [CheckPermission(PermissionProvider.Configuration.MANAGE_USER)]
    public async Task<IActionResult> Create()
    {
        var model = new UserRegistrationModel();
        await InitModelAsync(model);

        return View(model);
    }

    [HttpPost, FormName("save-continue", "continueEditing")]
    [CheckPermission(PermissionProvider.Configuration.MANAGE_USER)]
    public async Task<IActionResult> Create(UserRegistrationModel model, bool continueEditing)
    {
        if (ModelState.IsValid)
        {
            var loggedUser = await _workContext.GetCurrentUserAsync();
            var userEntity = _mapper.Map<User>(model.User);

            var status = await _userService.RegisterAsync(userEntity, model.User.SelectedRoleIds,
                loggedUser, model.User.EmailWelcomeKit);

            switch (status)
            {
                case RegistrationResultEnum.InvalidEmpCode:
                    ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync("Error.UnableToRegisterUser"));
                    break;
            }

            if (status == RegistrationResultEnum.Successful)
            {
                await _userActivityService.InsertAsync("User", string.Format(await _localizationService.GetResourceAsync("Log.RecordCreated"), userEntity.Name), userEntity);
                return continueEditing ? RedirectToAction("Edit", new { id = userEntity.Id }) : RedirectToAction("Index");
            }

            await _userActivityService.InsertAsync("User", string.Format(await _localizationService.GetResourceAsync("Log.RecordError"), userEntity.Name), userEntity);
            ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync("Error.UnableToRegisterUser"));
        }

        await InitModelAsync(model);
        return View(model);
    }

    [CheckPermission(PermissionProvider.Configuration.MANAGE_USER)]
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _userService.GetByIdAsync(id);
        if (entity == null)
            return RedirectToAction("Index");

        var model = new UserRegistrationModel
        {
            User = _mapper.Map<UserModel>(entity)
        };

        model.User.SelectedRoleIds = entity.UserRoleMaps.Select(x => x.UserRoleId).ToList();

        await InitModelAsync(model);

        return View(model);
    }

    [HttpPost]
    [CheckPermission(PermissionProvider.Configuration.MANAGE_USER)]
    public async Task<IActionResult> Edit(UserRegistrationModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = await _userService.GetByIdAsync(model.User.Id);
            entity = _mapper.Map(model.User, entity);

            #region Roles

            var avaliableRoles = await _userService.GetAllUserRolesAsync();
            var activeRoles = avaliableRoles.Where(x => entity.UserRoleMaps.Any(y => y.UserRoleId == x.Id)).ToList();
            var selectedRoles = avaliableRoles.Where(x => model.User.SelectedRoleIds.Any(y => y == x.Id)).ToList();
            var rolesToAdd = selectedRoles.Except(activeRoles);
            var rolesToDelete = activeRoles.Except(selectedRoles);

            foreach (var role in rolesToDelete)
                entity.RemoveFromRole(entity.UserRoleMaps.First(x => x.UserRoleId == role.Id));

            foreach (var role in rolesToAdd)
                entity.AddToRole(new UserRoleMap { UserId = entity.Id, UserRoleId = role.Id });

            #endregion

            await _userService.UpdateAsync(entity);
            await _userActivityService.InsertAsync("User", string.Format(await _localizationService.GetResourceAsync("Log.RecordUpdated"), entity.Name), entity);

            return RedirectToAction("Index");
        }

        ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync("Error.UnableToUpdateUser"));

        await InitModelAsync(model);

        return View(model);
    }

    [CheckPermission(PermissionProvider.Configuration.MANAGE_USER)]
    public async Task<IActionResult> ResetPassword(int id)
    {
        var model = new ResetPasswordModel { Id = id };

        return PartialView(model);
    }

    [HttpPost]
    [CheckPermission(PermissionProvider.Configuration.MANAGE_USER)]
    public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
    {
        if (ModelState.IsValid)
        {
            await _userService.ResetPasswordAsync(model.Id, model.Password);

            var user = await _userService.GetByIdAsync(model.Id);
            await _userActivityService.InsertAsync("ResetPassword", string.Format(await _localizationService.GetResourceAsync("Log.PasswordReset"), user.Name));

            return Json(new JsonResponseModel
            {
                Status = HttpStatusCodeEnum.Success,
                Message = await _localizationService.GetResourceAsync("Message.SaveSuccess")
            });
        }

        return Json(new JsonResponseModel
        {
            Status = ModelState.IsValid ? HttpStatusCodeEnum.InternalServerError : HttpStatusCodeEnum.ValidationError,
            Message = await _localizationService.GetResourceAsync("Error.Failed"),
            Errors = ModelState.AllErrors()
        });
    }

    #endregion

    #region Data

    [HttpPost]
    [CheckPermission(PermissionProvider.Configuration.MANAGE_USER)]
    public async Task<IActionResult> DataRead(DataTableRequest request)
    {
        var data = await _userService.GetPagedListAsync(request.SearchValue, request.Start,
            request.Length, request.SortColumn, request.SortDirection);

        return Json(new
        {
            request.Draw,
            data = data.Select(x => _mapper.Map<UserModel>(x)),
            recordsFiltered = data.TotalCount,
            recordsTotal = data.TotalCount
        });
    }

    #endregion

    #region Helper

    private async Task InitModelAsync(UserRegistrationModel model)
    {
        var languages = await _languageService.GetAllActiveAsync();
        var userRoles = await _userService.GetAllUserRolesAsync();

        foreach (var item in languages)
        {
            model.AvailableLanguages.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString(),
                Selected = item.Id == model.User.LanguageId
            });
        }

        foreach (var item in userRoles)
        {
            model.AvailableRoles.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString(),
                Selected = model.User.SelectedRoleIds.Contains(item.Id)
            });
        }
    }

    #endregion
}