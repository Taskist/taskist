using Microsoft.AspNetCore.Mvc.Rendering;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Common;

namespace Taskist.Web.Models.Masters;

public class ProjectRepositoryModel : BaseModel
{
    public int ProjectId { get; set; }

    [LocalizedDisplayName("ProjectRepositoryModel.GitConnectionId")]
    public int GitConnectionId { get; set; }

    [LocalizedDisplayName("ProjectRepositoryModel.RepoRef")]
    public string? RepoRef { get; set; }

    public string? RepoName { get; set; }

    [LocalizedDisplayName("ProjectRepositoryModel.IsPrimary")]
    public bool IsPrimary { get; set; }

    [LocalizedDisplayName("ProjectRepositoryModel.Active")]
    public bool Active { get; set; } = true;

    [LocalizedDisplayName("ProjectRepositoryModel.IssueSyncMode")]
    public int IssueSyncMode { get; set; }

    [LocalizedDisplayName("ProjectRepositoryModel.IssueImportLabel")]
    public string? IssueImportLabel { get; set; }

    [LocalizedDisplayName("ProjectRepositoryModel.IssueSyncTaskTypeIds")]
    public List<int> IssueSyncTaskTypeIds { get; set; } = [];

    [LocalizedDisplayName("ProjectRepositoryModel.IssueSyncComments")]
    public bool IssueSyncComments { get; set; }

    [LocalizedDisplayName("ProjectRepositoryModel.IssueTaskTypeId")]
    public int? IssueTaskTypeId { get; set; }

    [LocalizedDisplayName("ProjectRepositoryModel.IssueSeverityId")]
    public int? IssueSeverityId { get; set; }

    [LocalizedDisplayName("ProjectRepositoryModel.IssueOpenStatusId")]
    public int? IssueOpenStatusId { get; set; }

    [LocalizedDisplayName("ProjectRepositoryModel.IssueClosedStatusId")]
    public int? IssueClosedStatusId { get; set; }

    [LocalizedDisplayName("ProjectRepositoryModel.IssueSyncUserId")]
    public int? IssueSyncUserId { get; set; }

    [LocalizedDisplayName("ProjectRepositoryModel.IssueImportLimitPerHour")]
    public int IssueImportLimitPerHour { get; set; } = 50;

    public bool? IsPrivate { get; set; }

    public IList<SelectListItem> AvailableConnections { get; set; } = [];

    public IList<SelectListItem> AvailableSyncModes { get; set; } = [];

    public IList<SelectListItem> AvailableTaskTypes { get; set; } = [];

    public IList<SelectListItem> AvailableSeverities { get; set; } = [];

    public IList<SelectListItem> AvailableStatuses { get; set; } = [];

    public IList<SelectListItem> AvailableUsers { get; set; } = [];
}
