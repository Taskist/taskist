using Taskist.Core.Dtos.WorkItems;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Common;

namespace Taskist.Web.Models.WorkItems;

public class ClosingCommentModel : BaseModel
{
    [LocalizedDisplayName("Backlog.RootCauseAnalysis")]
    public string? RootCauseAnalysis { get; set; }

    [LocalizedDisplayName("ClosingCommentModel.ResolutionDetails")]
    public string? ResolutionDetails { get; set; }

    [LocalizedDisplayName("ClosingCommentModel.MergeRequestId")]
    public int? MergeRequestId { get; set; }

    public int? MergeRequestRepositoryId { get; set; }

    public int Status { get; set; }

    public string? StatusName { get; set; }

    public string? TaskTitle { get; set; }

    public string? GitLabWarning { get; set; }

    public string? GitLabProjectUrl { get; set; }

    public ClosingRequirementDto Requirement { get; set; } = new();

    public IList<ClosingMergeRequestOption> AvailableMergeRequests { get; set; } = [];
}
