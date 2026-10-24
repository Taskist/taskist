using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Taskist.Core.Domain.Git;
using Taskist.Core.Enums;
using Taskist.Service.Git;
using Taskist.Service.Localization;
using Taskist.Service.Security;
using Taskist.Web.Controllers.Common;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Common;
using Taskist.Web.Models.Datatable;
using Taskist.Web.Models.Masters;

namespace Taskist.Web.Controllers.Masters;

[CheckPermission(nameof(PermissionProvider.ManageGitConnections))]
public class GitConnectionController : BaseController
{
    #region Fields

    protected readonly IGitRepositoryService _gitRepositoryService;
    protected readonly IGitLegacyUpgradeService _gitLegacyUpgradeService;
    protected readonly ILocalizationService _localizationService;

    #endregion

    #region Ctor

    public GitConnectionController(IGitRepositoryService gitRepositoryService,
        IGitLegacyUpgradeService gitLegacyUpgradeService,
        ILocalizationService localizationService)
    {
        _gitRepositoryService = gitRepositoryService;
        _gitLegacyUpgradeService = gitLegacyUpgradeService;
        _localizationService = localizationService;
    }

    #endregion

    #region Actions

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Create()
    {
        var model = new GitConnectionModel { BaseUrl = "https://github.com", IsDefault = !(await _gitRepositoryService.GetConnectionsAsync(false)).Any() };
        await InitModelAsync(model);

        return PartialView(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(GitConnectionModel model)
    {
        if (!ModelState.IsValid)
            return Errors();

        var entity = new GitConnection();
        Apply(model, entity);

        var error = await VerifyAsync(entity);
        if (error != null)
            return Errors(error);

        await _gitRepositoryService.InsertConnectionAsync(entity);

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCode.Success,
            Message = string.Format(await _localizationService.GetResourceAsync("GitConnection.Verified"), entity.ServiceAccountUserName)
        });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _gitRepositoryService.GetConnectionByIdAsync(id);
        if (entity == null)
            return NoDataPartial();

        var model = new GitConnectionModel
        {
            Id = entity.Id,
            Name = entity.Name,
            ProviderType = entity.ProviderType,
            BaseUrl = entity.BaseUrl,
            ApiUrl = entity.ApiUrl,
            AuthMode = entity.AuthMode,
            HasServiceToken = !string.IsNullOrEmpty(entity.ServiceToken),
            AppId = entity.AppId,
            HasAppPrivateKey = !string.IsNullOrEmpty(entity.AppPrivateKey),
            OAuthClientId = entity.OAuthClientId,
            HasOAuthClientSecret = !string.IsNullOrEmpty(entity.OAuthClientSecret),
            AllowInsecureHttp = entity.AllowInsecureHttp,
            IsDefault = entity.IsDefault,
            Active = entity.Active,
            ServiceAccountUserName = entity.ServiceAccountUserName
        };
        await InitModelAsync(model);

        return PartialView(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(GitConnectionModel model)
    {
        if (!ModelState.IsValid)
            return Errors();

        var entity = await _gitRepositoryService.GetConnectionByIdAsync(model.Id);
        if (entity == null)
            return Errors(await _localizationService.GetResourceAsync("FormNoData.Description"));

        Apply(model, entity);

        if (entity.Active)
        {
            var error = await VerifyAsync(entity);
            if (error != null)
                return Errors(error);
        }

        await _gitRepositoryService.UpdateConnectionAsync(entity);

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCode.Success,
            Message = await _localizationService.GetResourceAsync("Message.UpdateSuccess")
        });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _gitRepositoryService.GetConnectionByIdAsync(id);
        if (entity == null)
            return Json(new JsonResponseModel
            {
                Status = HttpStatusCode.NoData,
                Message = await _localizationService.GetResourceAsync("FormNoData.Description")
            });

        await _gitRepositoryService.DeleteConnectionAsync(entity);

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCode.Success,
            Message = await _localizationService.GetResourceAsync("Message.DeleteSuccess")
        });
    }

    [HttpPost]
    public async Task<IActionResult> ReEncrypt()
    {
        var count = await _gitLegacyUpgradeService.ReEncryptSecretsAsync();

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCode.Success,
            Message = string.Format(await _localizationService.GetResourceAsync("GitConnection.ReEncrypted"), count)
        });
    }

    #endregion

    #region Data

    [HttpPost]
    public async Task<IActionResult> DataRead(DataTableRequest request)
    {
        var connections = await _gitRepositoryService.GetConnectionsAsync(false);

        return Json(new
        {
            request.Draw,
            data = connections.Select(x => new GitConnectionGridModel
            {
                Id = x.Id,
                Name = x.Name,
                Provider = ((GitProviderType)x.ProviderType).ToString(),
                BaseUrl = x.BaseUrl,
                AuthMode = x.AuthMode == (int)GitAuthMode.GitHubApp ? "GitHub App" : "Access token",
                ServiceAccount = x.ServiceAccountUserName,
                OAuth = !string.IsNullOrEmpty(x.OAuthClientId),
                IsDefault = x.IsDefault,
                Active = x.Active
            }),
            recordsFiltered = connections.Count,
            recordsTotal = connections.Count
        });
    }

    #endregion

    #region Utilities

    private static void Apply(GitConnectionModel model, GitConnection entity)
    {
        entity.Name = model.Name!.Trim();
        entity.ProviderType = model.ProviderType;
        entity.BaseUrl = model.BaseUrl!.Trim().TrimEnd('/');
        entity.ApiUrl = string.IsNullOrWhiteSpace(model.ApiUrl) ? null : model.ApiUrl.Trim();
        entity.AuthMode = model.AuthMode;
        entity.AppId = string.IsNullOrWhiteSpace(model.AppId) ? null : model.AppId.Trim();
        entity.OAuthClientId = string.IsNullOrWhiteSpace(model.OAuthClientId) ? null : model.OAuthClientId.Trim();
        entity.AllowInsecureHttp = model.AllowInsecureHttp;
        entity.IsDefault = model.IsDefault;
        entity.Active = model.Active;

        if (!string.IsNullOrWhiteSpace(model.ServiceToken))
            entity.ServiceToken = model.ServiceToken.Trim();
        if (!string.IsNullOrWhiteSpace(model.AppPrivateKey))
            entity.AppPrivateKey = model.AppPrivateKey.Trim();
        if (!string.IsNullOrWhiteSpace(model.OAuthClientSecret))
            entity.OAuthClientSecret = model.OAuthClientSecret.Trim();

        if (model.ClearOAuth)
        {
            entity.OAuthClientId = null;
            entity.OAuthClientSecret = null;
        }
    }

    private async Task<string?> VerifyAsync(GitConnection entity)
    {
        if (entity.AuthMode == (int)GitAuthMode.GitHubApp)
        {
            if (string.IsNullOrWhiteSpace(entity.AppId) || string.IsNullOrWhiteSpace(entity.AppPrivateKey))
                return await _localizationService.GetResourceAsync("GitConnectionModel.AppCredentials.RequiredMsg");
        }
        else if (string.IsNullOrWhiteSpace(entity.ServiceToken))
            return await _localizationService.GetResourceAsync("GitConnectionModel.ServiceToken.RequiredMsg");

        var identity = await _gitRepositoryService.ProviderFor(entity).GetServiceIdentityAsync();
        if (!identity.Success || identity.Data == null)
            return identity.Error ?? await _localizationService.GetResourceAsync("GitConnection.VerifyFailed");

        entity.ServiceAccountUserName = identity.Data.UserName;
        entity.ServiceAccountExternalId = identity.Data.Id > 0 ? identity.Data.Id.ToString() : null;
        return null;
    }

    private async Task InitModelAsync(GitConnectionModel model)
    {
        model.OAuthCallbackUrl = Url.Action("GitOAuthCallback", "Profile", null, Request.Scheme);

        foreach (var type in Enum.GetValues<GitProviderType>())
            model.AvailableProviders.Add(new SelectListItem { Text = type.ToString(), Value = ((int)type).ToString(), Selected = (int)type == model.ProviderType });

        model.AvailableAuthModes.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("GitAuthMode.AccessToken"), Value = ((int)GitAuthMode.AccessToken).ToString() });
        model.AvailableAuthModes.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("GitAuthMode.GitHubApp"), Value = ((int)GitAuthMode.GitHubApp).ToString() });
    }

    private JsonResult Errors(string? message = null)
    {
        if (!string.IsNullOrEmpty(message))
            ModelState.AddModelError(string.Empty, message);

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCode.ValidationError,
            Message = message ?? _localizationService.GetResource("Error.Failed"),
            Errors = ModelState.AllErrors()
        });
    }

    #endregion
}
