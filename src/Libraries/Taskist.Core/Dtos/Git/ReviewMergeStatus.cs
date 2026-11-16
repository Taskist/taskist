namespace Taskist.Core.Dtos.Git;

public static class ReviewMergeStatus
{
    #region Fields

    public const string Mergeable = "mergeable";

    public const string Conflict = "conflict";

    public const string Draft = "draft";

    public const string NeedsRebase = "needs-rebase";

    public const string Checking = "checking";

    public const string NotChecked = "not-checked";

    public const string Blocked = "blocked";

    public const string Other = "other";

    #endregion

    #region Methods

    public static string From(ReviewMergeRequestDto mr)
    {
        if (mr.IsDraft)
            return Draft;
        if (mr.HasConflicts)
            return Conflict;

        switch (mr.DetailedMergeStatus?.ToLowerInvariant())
        {
            case "mergeable":
                return Mergeable;
            case "conflict":
            case "broken_status":
                return Conflict;
            case "need_rebase":
                return NeedsRebase;
            case "draft_status":
                return Draft;
            case "unchecked":
                return NotChecked;
            case "checking":
            case "preparing":
            case "approvals_syncing":
                return Checking;
            case null:
            case "":
                break;
            default:
                return Blocked;
        }

        return mr.MergeStatus?.ToLowerInvariant() switch
        {
            "can_be_merged" => Mergeable,
            "cannot_be_merged" or "cannot_be_merged_recheck" => Conflict,
            "unchecked" => NotChecked,
            "checking" or "cannot_be_merged_rechecking" => Checking,
            _ => Other
        };
    }

    #endregion
}
