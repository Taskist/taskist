using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Taskist.Core.Domain.Announcements;
using Taskist.Core.Enums;
using Taskist.Service.Announcements;
using Taskist.Service.Common;
using Taskist.Service.Localization;
using Taskist.Service.Security;
using Taskist.Service.Users;
using Taskist.Web.Controllers.Common;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Helpers.Mapping;
using Taskist.Web.Models.Announcements;
using Taskist.Web.Models.Common;
using Taskist.Web.Models.Datatable;

namespace Taskist.Web.Controllers.Announcements;

[CheckPermission(nameof(PermissionProvider.ManageAnnouncement))]
public class AnnouncementController : BaseController
{
    #region Fields

    protected readonly IAnnouncementService _announcementService;
    protected readonly IUserService _userService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IRealtimeAlertNotifier _notifier;
    protected readonly AnnouncementMapper _mapper;

    #endregion

    #region Ctor

    public AnnouncementController(IAnnouncementService announcementService,
        IUserService userService,
        ILocalizationService localizationService,
        IRealtimeAlertNotifier notifier,
        AnnouncementMapper mapper)
    {
        _announcementService = announcementService;
        _userService = userService;
        _localizationService = localizationService;
        _notifier = notifier;
        _mapper = mapper;
    }

    #endregion

    #region Actions

    public IActionResult Index() => View();

    public async Task<IActionResult> Create()
    {
        var model = new AnnouncementModel
        {
            ValidFrom = DateOnly.FromDateTime(DateTime.Today),
            ValidTo = DateOnly.FromDateTime(DateTime.Today).AddMonths(1)
        };
        await InitModelAsync(model);

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(AnnouncementModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = _mapper.ToEntity(model);
            await _announcementService.InsertAsync(entity, model.SelectedRoleIds);
            await NotifyAudienceAsync(entity);

            return RedirectToAction("Index");
        }

        ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync("Error.Failed"));
        await InitModelAsync(model);
        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _announcementService.GetByIdAsync(id);
        if (entity == null)
            return NoData();

        var model = _mapper.ToModel(entity);
        model.SelectedRoleIds = await _announcementService.GetRoleIdsAsync(id);
        await InitModelAsync(model);

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(AnnouncementModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = await _announcementService.GetByIdAsync(model.Id);
            _mapper.Update(model, entity);
            await _announcementService.UpdateAsync(entity, model.SelectedRoleIds);
            await NotifyAudienceAsync(entity);

            return RedirectToAction("Index");
        }

        ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync("Error.Failed"));
        await InitModelAsync(model);
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _announcementService.GetByIdAsync(id);
        if (entity == null)
            return Json(new JsonResponseModel
            {
                Status = HttpStatusCode.NoData,
                Message = await _localizationService.GetResourceAsync("FormNoData.Description")
            });

        await _announcementService.DeleteAsync(entity);
        return Json(new JsonResponseModel
        {
            Status = HttpStatusCode.Success,
            Message = await _localizationService.GetResourceAsync("Message.DeleteSuccess")
        });
    }

    public async Task<IActionResult> Acknowledgements(int id)
    {
        var entity = await _announcementService.GetByIdAsync(id);
        if (entity == null)
            return NoDataPartial();

        var model = new AnnouncementAckAuditModel { Title = entity.Title };
        foreach (var ack in await _announcementService.GetAcknowledgementsAsync(id))
            model.Rows.Add(new AnnouncementAckRowModel
            {
                User = ack.User?.Name,
                AcknowledgedOn = ack.AcknowledgedOnUtc.ToLocalTime()
            });

        return PartialView(model);
    }

    #endregion

    #region Data

    [HttpPost]
    public async Task<IActionResult> DataRead(DataTableRequest request)
    {
        var data = await _announcementService.GetPagedListAsync(request.SearchValue, request.Start,
            request.Length, request.SortColumn, request.SortDirection);

        return Json(new
        {
            request.Draw,
            data = data.Select(x => _mapper.ToModel(x)),
            recordsFiltered = data.TotalCount,
            recordsTotal = data.TotalCount
        });
    }

    #endregion

    #region Helpers

    private async Task InitModelAsync(AnnouncementModel model)
    {
        model.AvailableTypes = model.Type.ToSelectList();
        model.AvailableSeverities = model.Severity.ToSelectList();
        model.AvailableAudiences = model.Audience.ToSelectList();

        foreach (var r in await _userService.GetAllUserRolesAsync(true))
            model.AvailableRoles.Add(new SelectListItem { Text = r.Name, Value = r.Id.ToString() });
    }

    private async Task NotifyAudienceAsync(Announcement entity)
    {
        var users = await _userService.GetAllActiveAsync();
        var ids = users.Select(e => e.Id).ToArray();
        if (ids.Length > 0)
            await _notifier.NotifyAsync(ids);
    }

    #endregion
}
