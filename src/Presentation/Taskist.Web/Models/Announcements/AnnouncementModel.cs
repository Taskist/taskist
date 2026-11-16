using Microsoft.AspNetCore.Mvc.Rendering;
using Taskist.Core.Enums;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Common;

namespace Taskist.Web.Models.Announcements;

public class AnnouncementModel : BaseModel
{
    public AnnouncementModel()
    {
        AvailableTypes = [];
        AvailableSeverities = [];
        AvailableAudiences = [];
        AvailableRoles = [];
        SelectedRoleIds = [];
    }

    [LocalizedDisplayName("AnnouncementModel.Title")]
    public string Title { get; set; }

    [LocalizedDisplayName("AnnouncementModel.Body")]
    public string Body { get; set; }

    [LocalizedDisplayName("AnnouncementModel.Type")]
    public AnnouncementType Type { get; set; }

    [LocalizedDisplayName("AnnouncementModel.Severity")]
    public AnnouncementSeverity Severity { get; set; }

    [LocalizedDisplayName("AnnouncementModel.Audience")]
    public AnnouncementAudience Audience { get; set; }

    [LocalizedDisplayName("AnnouncementModel.ValidFrom")]
    public DateOnly ValidFrom { get; set; }

    [LocalizedDisplayName("AnnouncementModel.ValidTo")]
    public DateOnly ValidTo { get; set; }

    [LocalizedDisplayName("AnnouncementModel.RequiresAcknowledgement")]
    public bool RequiresAcknowledgement { get; set; }

    [LocalizedDisplayName("AnnouncementModel.DisplayOrder")]
    public int DisplayOrder { get; set; }

    [LocalizedDisplayName("AnnouncementModel.Active")]
    public bool Active { get; set; } = true;

    public string TypeName => Type.ToString();

    public string SeverityName => Severity.ToString();

    public string AudienceName => Audience.ToString();

    public int[] SelectedRoleIds { get; set; }

    public IList<SelectListItem> AvailableTypes { get; set; }

    public IList<SelectListItem> AvailableSeverities { get; set; }

    public IList<SelectListItem> AvailableAudiences { get; set; }

    public IList<SelectListItem> AvailableRoles { get; set; }
}
