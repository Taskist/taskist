using Newtonsoft.Json;

namespace Taskist.Core.Dtos.Git;

public class ReviewMergeRequestDto
{
    [JsonProperty("iid")]
    public int Iid { get; set; }

    [JsonProperty("project_id")]
    public long ProjectId { get; set; }

    [JsonProperty("title")]
    public string? Title { get; set; }

    [JsonProperty("description")]
    public string? Description { get; set; }

    [JsonProperty("state")]
    public string? State { get; set; }

    [JsonProperty("draft")]
    public bool Draft { get; set; }

    [JsonProperty("work_in_progress")]
    public bool WorkInProgress { get; set; }

    [JsonProperty("detailed_merge_status")]
    public string? DetailedMergeStatus { get; set; }

    [JsonProperty("merge_status")]
    public string? MergeStatus { get; set; }

    [JsonProperty("has_conflicts")]
    public bool HasConflicts { get; set; }

    [JsonProperty("blocking_discussions_resolved")]
    public bool BlockingDiscussionsResolved { get; set; } = true;

    [JsonProperty("source_branch")]
    public string? SourceBranch { get; set; }

    [JsonProperty("target_branch")]
    public string? TargetBranch { get; set; }

    [JsonProperty("source_project_id")]
    public long SourceProjectId { get; set; }

    [JsonProperty("target_project_id")]
    public long TargetProjectId { get; set; }

    [JsonProperty("author")]
    public GitPersonDto? Author { get; set; }

    [JsonProperty("reviewers")]
    public List<GitPersonDto> Reviewers { get; set; } = [];

    [JsonProperty("assignees")]
    public List<GitPersonDto> Assignees { get; set; } = [];

    [JsonProperty("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonProperty("web_url")]
    public string? WebUrl { get; set; }

    [JsonProperty("user_notes_count")]
    public int UserNotesCount { get; set; }

    [JsonProperty("sha")]
    public string? Sha { get; set; }

    [JsonProperty("squash")]
    public bool Squash { get; set; }

    [JsonProperty("force_remove_source_branch")]
    public bool? ForceRemoveSourceBranch { get; set; }

    [JsonProperty("changes_count")]
    public string? ChangesCount { get; set; }

    [JsonProperty("merge_error")]
    public string? MergeError { get; set; }

    [JsonProperty("rebase_in_progress")]
    public bool RebaseInProgress { get; set; }

    [JsonProperty("diverged_commits_count")]
    public int? DivergedCommitsCount { get; set; }

    public bool IsDraft => Draft || WorkInProgress;
}
