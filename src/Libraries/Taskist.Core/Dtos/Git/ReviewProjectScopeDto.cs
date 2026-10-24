namespace Taskist.Core.Dtos.Git;

public class ReviewProjectScopeDto
{
    public int ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public int RepositoryId { get; set; }

    public string RepositoryName { get; set; } = string.Empty;

    public int ConnectionId { get; set; }

    public int ProviderType { get; set; }

    public string GitProjectReference { get; set; } = string.Empty;

    public bool IsTeamLead { get; set; }

    public bool IsCodeReviewer { get; set; }

    public bool CanReview { get; set; }

    public bool CanMerge { get; set; }

    public bool CanRebase { get; set; }

    public string Term { get; set; } = "merge request";

    public string ShortTerm { get; set; } = "MR";

    public string RefPrefix { get; set; } = "!";

    public string DisplayName => string.Equals(RepositoryName, ProjectName, StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(RepositoryName)
        ? ProjectName
        : $"{ProjectName} / {RepositoryName}";
}
