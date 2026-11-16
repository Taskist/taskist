using Microsoft.AspNetCore.Mvc.Rendering;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Common;

namespace Taskist.Web.Models.Masters;

public class ProjectNotificationChannelModel : BaseModel
{
    public int ProjectId { get; set; }

    [LocalizedDisplayName("ProjectNotificationChannelModel.NotificationChannelId")]
    public int NotificationChannelId { get; set; }

    [LocalizedDisplayName("ProjectNotificationChannelModel.Events")]
    public List<int> Events { get; set; } = [];

    [LocalizedDisplayName("ProjectNotificationChannelModel.Active")]
    public bool Active { get; set; } = true;

    public IList<SelectListItem> AvailableChannels { get; set; } = [];

    public IList<SelectListItem> AvailableEvents { get; set; } = [];
}
