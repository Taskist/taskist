using Newtonsoft.Json;

namespace Taskist.Core.Dtos.Git;

public class GitRepositoryDto
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("path_with_namespace")]
    public string PathWithNamespace { get; set; } = string.Empty;

    [JsonProperty("default_branch")]
    public string? DefaultBranch { get; set; }

    [JsonProperty("web_url")]
    public string? WebUrl { get; set; }

    [JsonProperty("visibility")]
    public string? Visibility { get; set; }

    [JsonIgnore]
    public bool IsPrivate => !string.Equals(Visibility, "public", StringComparison.OrdinalIgnoreCase);
}
