using Microsoft.AspNetCore.Mvc.Rendering;

namespace Taskist.Web.Models.Configuration;

public class IntegrationSettingsModel
{
    public int GitConnectionCount { get; set; }

    public bool KeyRingProtected { get; set; }

    public bool AllowBranchCreation { get; set; }

    public string? BranchTaskKeyPrefix { get; set; }

    public int? MergedStatusId { get; set; }

    public int? RejectedStatusId { get; set; }

    public bool MergedStatusMissing { get; set; }

    public bool RejectedStatusMissing { get; set; }

    public int? OpenedStatusId { get; set; }

    public bool OpenedStatusMissing { get; set; }

    public bool MoveTasksOnMergeRequestOpen { get; set; } = true;

    public int SharedBranchFallbackLimit { get; set; } = 3;

    public bool AutoAssignTeamLeadReviewer { get; set; }

    public IList<SelectListItem> AvailableStatuses { get; set; } = [];
}
