using Microsoft.AspNetCore.Mvc;
using Taskist.Service.Announcements;
using Taskist.Web.Controllers.Common;

namespace Taskist.Web.Controllers.Announcements;

public class AnnouncementFeedController : BaseController
{
    #region Fields

    protected readonly IAnnouncementService _announcementService;

    #endregion

    #region Ctor

    public AnnouncementFeedController(IAnnouncementService announcementService)
    {
        _announcementService = announcementService;
    }

    #endregion

    #region Actions

    [HttpPost]
    public async Task<IActionResult> Acknowledge(int id)
    {
        await _announcementService.AcknowledgeAsync(id, UserId);
        return Json(new { ok = true });
    }

    #endregion
}
