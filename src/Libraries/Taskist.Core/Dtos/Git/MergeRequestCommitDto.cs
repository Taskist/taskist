using Newtonsoft.Json;

namespace Taskist.Core.Dtos.Git;

public class MergeRequestCommitDto
{
    [JsonProperty("short_id")]
    public string? ShortId { get; set; }

    [JsonProperty("title")]
    public string? Title { get; set; }

    [JsonProperty("author_name")]
    public string? AuthorName { get; set; }

    [JsonProperty("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("web_url")]
    public string? WebUrl { get; set; }
}
