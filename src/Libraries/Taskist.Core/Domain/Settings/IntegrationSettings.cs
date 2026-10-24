using Taskist.Core.Domain.Common;

namespace Taskist.Core.Domain.Settings;

public class IntegrationSettings : ISettings
{
    public string? GitLabBaseUrl { get; set; }

    public string? GitLabAccessToken { get; set; }

    public bool AllowBranchCreation { get; set; }

    public string? BranchTaskKeyPrefix { get; set; }

    public string? WebhookSecret { get; set; }

    public int? MergedStatusId { get; set; }

    public int? RejectedStatusId { get; set; }

    public int? OpenedStatusId { get; set; }

    public bool MoveTasksOnMergeRequestOpen { get; set; } = true;

    public int SharedBranchFallbackLimit { get; set; } = 3;

    public bool AutoAssignTeamLeadReviewer { get; set; }
}
