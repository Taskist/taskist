namespace Taskist.Core.Dtos.Git;

public class ReviewDetailDto
{
    public ReviewProjectScopeDto Scope { get; set; } = new();

    public ReviewMergeRequestDto MergeRequest { get; set; } = new();

    public string Status { get; set; } = ReviewMergeStatus.Other;

    public List<string> ApprovedBy { get; set; } = [];

    public bool UserHasApproved { get; set; }

    public List<MergeRequestCommitDto> Commits { get; set; } = [];

    public List<MergeRequestNoteDto> Notes { get; set; } = [];

    public List<ReviewLinkedTaskDto> Tasks { get; set; } = [];

    public bool Connected { get; set; }

    public bool IsOwn { get; set; }

    public bool TitleIsTaskReference { get; set; }

    public bool CanUnapprove { get; set; } = true;

    public string RebaseLabel { get; set; } = "Rebase";

    public bool CanAct => Connected && Scope.CanReview && MergeRequest.State == "opened";

    public bool CanMergeNow => Connected && Scope.CanMerge && MergeRequest.State == "opened" && Status == ReviewMergeStatus.Mergeable;

    public string? MergeBlockedReason { get; set; }
}
