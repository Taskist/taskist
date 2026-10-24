using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using Taskist.Core.Common;
using Taskist.Core.Domain.Users;
using Taskist.Core.Enums;
using Taskist.Service.Git;
using Taskist.Service.Localization;
using Taskist.Service.Masters;
using Taskist.Service.Security;
using Taskist.Service.Users;
using Taskist.Web.Controllers.Common;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Helpers.Mapping;
using Taskist.Web.Models.Common;
using Taskist.Web.Models.Datatable;
using Taskist.Web.Models.Users;

namespace Taskist.Web.Controllers.Users;

[CheckPermission(nameof(PermissionProvider.ManageUser))]
public class UserController : BaseController
{
    #region Fields

    protected readonly IUserService _userService;
    protected readonly IDocumentService _documentService;
    protected readonly ILanguageService _languageService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IGitRepositoryService _gitRepositoryService;
    protected readonly IWorkContext _workContext;
    protected readonly UsersMapper _usersMapper;
    protected readonly MastersMapper _mastersMapper;

    #endregion

    #region Ctor

    public UserController(IUserService userService,
        IDocumentService documentService,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IGitRepositoryService gitRepositoryService,
        IWorkContext workContext,
        UsersMapper usersMapper,
        MastersMapper mastersMapper)
    {
        _userService = userService;
        _documentService = documentService;
        _languageService = languageService;
        _localizationService = localizationService;
        _gitRepositoryService = gitRepositoryService;
        _workContext = workContext;
        _usersMapper = usersMapper;
        _mastersMapper = mastersMapper;
    }

    #endregion

    #region Action

    public async Task<IActionResult> Index()
    {
        var model = new UserListModel();

        foreach (var s in Enum.GetValues<UserStatus>())
            model.AvailableStatuses.Add(new SelectListItem { Text = s.ToString(), Value = ((int)s).ToString(), Selected = s == UserStatus.Active });

        foreach (var r in await _userService.GetAllUserRolesAsync())
            model.AvailableRoles.Add(new SelectListItem { Text = r.Name, Value = r.Id.ToString() });

        return View(model);
    }

    public async Task<IActionResult> Create()
    {
        var model = new UserRegistrationModel();
        await InitModelAsync(model);

        return View(model);
    }

    [HttpPost, FormName("save-continue", "continueEditing")]
    public async Task<IActionResult> Create(UserRegistrationModel model, bool continueEditing)
    {
        if (ModelState.IsValid)
        {
            var loggedUser = await _workContext.GetCurrentUserAsync();
            var userEntity = _usersMapper.ToEntity(model.User);
            var addressEntity = _mastersMapper.ToEntity(model.Address);

            var status = await _userService.RegisterAsync(userEntity, addressEntity, model.User.SelectedRoleIds,
                loggedUser, model.User.EmailWelcomeKit);

            switch (status)
            {
                case RegistrationResult.InvalidEmpCode:
                    ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync("Error.UnableToRegisterUser"));
                    break;
            }

            if (status == RegistrationResult.Successful)
            {
                return continueEditing ? RedirectToAction("Create") : RedirectToAction("Index");
            }

            ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync("Error.UnableToRegisterUser"));
        }

        await InitModelAsync(model);
        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _userService.GetByIdAsync(id);
        if (entity == null)
            return RedirectToAction("Index");

        var model = new UserRegistrationModel
        {
            User = _usersMapper.ToModel(entity),
            Address = _mastersMapper.ToModel(entity.Address)
        };

        model.User.SelectedRoleIds = entity.UserUserRoleMaps.Select(x => x.UserRoleId).ToList();

        await InitModelAsync(model);

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(UserRegistrationModel model)
    {
        if (ModelState.IsValid)
        {

            var entity = await _userService.GetByIdAsync(model.User.Id);
            _usersMapper.Update(model.User, entity);
            _mastersMapper.Update(model.Address, entity.Address);

            #region Roles

            var avaliableRoles = await _userService.GetAllUserRolesAsync();
            var activeRoles = avaliableRoles.Where(x => entity.UserUserRoleMaps.Any(y => y.UserRoleId == x.Id)).ToList();
            var selectedRoles = avaliableRoles.Where(x => model.User.SelectedRoleIds.Any(y => y == x.Id)).ToList();
            var rolesToAdd = selectedRoles.Except(activeRoles);
            var rolesToDelete = activeRoles.Except(selectedRoles);

            foreach (var role in rolesToDelete)
                entity.RemoveFromRole(entity.UserUserRoleMaps.First(x => x.UserRoleId == role.Id));

            foreach (var role in rolesToAdd)
                entity.AddToRole(new UserRoleMap { UserId = entity.Id, UserRoleId = role.Id });

            #endregion

            await _userService.UpdateAsync(entity);

            return RedirectToAction("Index");
        }

        ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync("Error.UnableToUpdateUser"));

        await InitModelAsync(model);

        return View(model);
    }

    public async Task<IActionResult> ResetPassword(int id)
    {
        var model = new ResetPasswordModel { Id = id };

        return PartialView(model);
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
    {
        if (ModelState.IsValid)
        {
            await _userService.ResetPasswordAsync(model.Id, model.Password);

            var user = await _userService.GetByIdAsync(model.Id);

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

    public async Task<IActionResult> Unlock(int id)
    {
        var entity = await _userService.GetByIdAsync(id);
        if (entity == null)
            return RedirectToAction("Index");

        var model = _usersMapper.ToModel(entity);

        return PartialView(model);
    }

    [HttpPost, ActionName("Unlock")]
    public async Task<IActionResult> UnlockConfirmed(int id)
    {
        var unlocked = await _userService.UnlockAsync(id);

        return Json(new JsonResponseModel
        {
            Status = unlocked ? HttpStatusCode.Success : HttpStatusCode.NoData,
            Message = await _localizationService.GetResourceAsync(unlocked
                ? "UserUnlock.SuccessMsg"
                : "FormNoData.Description")
        });
    }

    #endregion

    #region Data

    [HttpPost]
    public async Task<IActionResult> DataRead(DataTableRequest request, UserFilterModel filter)
    {
        filter ??= new UserFilterModel();
        var pageIndex = request.Length > 0 ? request.Start / request.Length : 0;

        var data = await _userService.GetPagedListAsync(request.SearchValue, pageIndex,
            request.Length, request.SortColumn, request.SortDirection,
            filter.StatusValues, filter.RoleValues, filter.LockedValue);

        return Json(new
        {
            request.Draw,
            data = data.Select(x => new
            {
                x.Id,
                x.FirstName,
                x.LastName,
                x.Email,
                x.MobileNumber,
                StatusName = ((UserStatus)x.Status).ToString(),
                x.Locked
            }),
            recordsFiltered = data.TotalCount,
            recordsTotal = data.TotalCount
        });
    }

    #endregion

    #region Helper

    public async Task<JsonResult> GetGitLabUsers(string q)
    {
        var connection = await _gitRepositoryService.GetDefaultConnectionAsync();
        if (connection == null)
            return Json(Array.Empty<SelectListItem>());

        var provider = _gitRepositoryService.ProviderFor(connection);
        var users = await provider.SearchUsersAsync(q);
        if (users.Count == 0 && !string.IsNullOrEmpty(provider.LastError))
            return Json(new { error = provider.LastError });

        return Json(users.Select(u => new SelectListItem
        {
            Value = u.UserName,
            Text = string.IsNullOrWhiteSpace(u.Name) ? u.UserName : $"{u.Name} ({u.UserName})"
        }));
    }

    private async Task InitModelAsync(UserRegistrationModel model)
    {
        model.User.GitLabConfigured = await _gitRepositoryService.GetDefaultConnectionAsync() != null;

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
