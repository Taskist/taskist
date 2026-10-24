using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Taskist.Core.Domain.Masters;
using Taskist.Core.Enums;
using Taskist.Service.Localization;
using Taskist.Service.Messages;
using Taskist.Service.Security;
using Taskist.Web.Controllers.Common;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Common;
using Taskist.Web.Models.Datatable;
using Taskist.Web.Models.Masters;

namespace Taskist.Web.Controllers.Masters;

[CheckPermission(nameof(PermissionProvider.ManageNotificationChannels))]
public class NotificationChannelController : BaseController
{
    #region Fields

    protected readonly INotificationChannelService _notificationChannelService;
    protected readonly ILocalizationService _localizationService;

    #endregion

    #region Ctor

    public NotificationChannelController(INotificationChannelService notificationChannelService,
        ILocalizationService localizationService)
    {
        _notificationChannelService = notificationChannelService;
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
        var model = new NotificationChannelModel();
        await InitModelAsync(model);

        return PartialView(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(NotificationChannelModel model)
    {
        if (!ModelState.IsValid)
            return Errors();

        var entity = new NotificationChannel();
        Apply(model, entity);

        var error = _notificationChannelService.ValidateEndpoint(entity.ChannelType, entity.EndpointUrl);
        if (error != null)
            return Errors(error);

        await _notificationChannelService.InsertChannelAsync(entity);

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCode.Success,
            Message = await _localizationService.GetResourceAsync("Message.SaveSuccess")
        });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _notificationChannelService.GetChannelByIdAsync(id);
        if (entity == null)
            return NoDataPartial();

        var model = new NotificationChannelModel
        {
            Id = entity.Id,
            Name = entity.Name,
            ChannelType = entity.ChannelType,
            HasEndpointUrl = !string.IsNullOrEmpty(entity.EndpointUrl),
            HasSigningSecret = !string.IsNullOrEmpty(entity.SigningSecret),
            Active = entity.Active,
            LastSentOn = entity.LastSentOn,
            LastError = entity.LastError
        };
        await InitModelAsync(model);

        return PartialView(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(NotificationChannelModel model)
    {
        if (!ModelState.IsValid)
            return Errors();

        var entity = await _notificationChannelService.GetChannelByIdAsync(model.Id);
        if (entity == null)
            return Errors(await _localizationService.GetResourceAsync("FormNoData.Description"));

        Apply(model, entity);

        var error = _notificationChannelService.ValidateEndpoint(entity.ChannelType, entity.EndpointUrl);
        if (error != null)
            return Errors(error);

        await _notificationChannelService.UpdateChannelAsync(entity);

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCode.Success,
            Message = await _localizationService.GetResourceAsync("Message.UpdateSuccess")
        });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _notificationChannelService.GetChannelByIdAsync(id);
        if (entity == null)
            return Json(new JsonResponseModel
            {
                Status = HttpStatusCode.NoData,
                Message = await _localizationService.GetResourceAsync("FormNoData.Description")
            });

        await _notificationChannelService.DeleteChannelAsync(entity);

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCode.Success,
            Message = await _localizationService.GetResourceAsync("Message.DeleteSuccess")
        });
    }

    [HttpPost]
    public async Task<IActionResult> Test(int id)
    {
        var entity = await _notificationChannelService.GetChannelByIdAsync(id);
        if (entity == null)
            return Json(new JsonResponseModel
            {
                Status = HttpStatusCode.NoData,
                Message = await _localizationService.GetResourceAsync("FormNoData.Description")
            });

        var (success, message) = await _notificationChannelService.TestAsync(entity);

        return Json(new JsonResponseModel
        {
            Status = success ? HttpStatusCode.Success : HttpStatusCode.ValidationError,
            Message = message
        });
    }

    #endregion

    #region Data

    [HttpPost]
    public async Task<IActionResult> DataRead(DataTableRequest request)
    {
        var typeNames = await ChannelTypeNamesAsync();
        var channels = await _notificationChannelService.GetChannelsAsync(false);

        return Json(new
        {
            request.Draw,
            data = channels.Select(x => new NotificationChannelGridModel
            {
                Id = x.Id,
                Name = x.Name,
                Type = typeNames.TryGetValue(x.ChannelType, out var type) ? type : x.ChannelType.ToString(),
                LastSent = x.LastSentOn?.ToLocalTime().ToString("dd-MMM-yyyy HH:mm"),
                LastError = x.LastError,
                Active = x.Active
            }),
            recordsFiltered = channels.Count,
            recordsTotal = channels.Count
        });
    }

    #endregion

    #region Utilities

    private static void Apply(NotificationChannelModel model, NotificationChannel entity)
    {
        entity.Name = model.Name!.Trim();
        entity.ChannelType = model.ChannelType;
        entity.Active = model.Active;

        if (!string.IsNullOrWhiteSpace(model.EndpointUrl))
        {
            entity.EndpointUrl = model.EndpointUrl.Trim();
            entity.LastError = null;
        }

        if (entity.ChannelType != (int)NotificationChannelType.GenericJson)
            entity.SigningSecret = null;
        else if (!string.IsNullOrWhiteSpace(model.SigningSecret))
            entity.SigningSecret = model.SigningSecret.Trim();
    }

    private async Task InitModelAsync(NotificationChannelModel model)
    {
        foreach (var item in await ChannelTypeNamesAsync())
            model.AvailableTypes.Add(new SelectListItem { Text = item.Value, Value = item.Key.ToString(), Selected = item.Key == model.ChannelType });
    }

    private async Task<Dictionary<int, string>> ChannelTypeNamesAsync()
    {
        var names = new Dictionary<int, string>();
        foreach (var value in Enum.GetValues<NotificationChannelType>())
            names[(int)value] = await _localizationService.GetResourceAsync($"NotificationChannelType.{value}");
        return names;
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
