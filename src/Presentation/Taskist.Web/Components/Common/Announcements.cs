using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Taskist.Core.Common;
using Taskist.Core.Enums;
using Taskist.Service.Announcements;
using Taskist.Web.Models.Announcements;

namespace Taskist.Web.Components.Common;

public class Announcements : ViewComponent
{
    #region Fields

    protected readonly IWorkContext _workContext;
    protected readonly IAnnouncementService _announcementService;

    #endregion

    #region Ctor

    public Announcements(IWorkContext workContext, IAnnouncementService announcementService)
    {
        _workContext = workContext;
        _announcementService = announcementService;
    }

    #endregion

    #region Methods

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var model = new AnnouncementDisplayModel();

        var userId = _workContext.GetCurrentUserId();
        if (userId == null)
            return View(model);

        foreach (var a in await _announcementService.GetActiveForUserAsync(userId.Value))
            model.Items.Add(new AnnouncementDisplayItemModel
            {
                Id = a.Id,
                Title = a.Title,
                Body = a.Body,
                Type = a.Type,
                Severity = a.Severity,
                RequiresAcknowledgement = a.RequiresAcknowledgement
            });

        model.OneTimeJson = JsonSerializer.Serialize(model.Items
            .Where(x => x.Type == AnnouncementType.OneTime)
            .Select(x => new { id = x.Id, title = x.Title, body = x.Body, severity = (int)x.Severity }));

        return View(model);
    }

    #endregion
}
