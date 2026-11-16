using Newtonsoft.Json;

namespace Taskist.Core.Dtos.Git;

public class MergeRequestDetailsDto
{
    [JsonProperty("iid")]
    public int MrId { get; set; }

    [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
    public string Title { get; set; }

    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
    public string Description { get; set; }

    [JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
    public string State { get; set; }

    [JsonProperty("created_at", NullValueHandling = NullValueHandling.Ignore)]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("updated_at", NullValueHandling = NullValueHandling.Ignore)]
    public DateTime UpdatedAt { get; set; }

    [JsonProperty("merged_at", NullValueHandling = NullValueHandling.Ignore)]
    public DateTime? MergedAt { get; set; }

    [JsonProperty("closed_at", NullValueHandling = NullValueHandling.Ignore)]
    public DateTime? ClosedAt { get; set; }

    [JsonProperty("target_branch", NullValueHandling = NullValueHandling.Ignore)]
    public string TargetBranch { get; set; }

    [JsonProperty("source_branch", NullValueHandling = NullValueHandling.Ignore)]
    public string SourceBranch { get; set; }

    [JsonProperty("author", NullValueHandling = NullValueHandling.Ignore)]
    public GitPersonDto Author { get; set; }

    [JsonProperty("draft", NullValueHandling = NullValueHandling.Ignore)]
    public bool Draft { get; set; }

    [JsonProperty("web_url", NullValueHandling = NullValueHandling.Ignore)]
    public string Url { get; set; }
}
