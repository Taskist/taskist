using Newtonsoft.Json;

namespace Taskist.Core.Dtos.Git;

public class BranchDto
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("default")]
    public bool Default { get; set; }

    [JsonProperty("protected")]
    public bool Protected { get; set; }

    [JsonProperty("web_url")]
    public string? Url { get; set; }

    [JsonProperty("merged")]
    public bool Merged { get; set; }

    [JsonProperty("commit")]
    public BranchCommitDto? Commit { get; set; }
}
