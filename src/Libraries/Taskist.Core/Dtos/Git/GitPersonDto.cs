using Newtonsoft.Json;

namespace Taskist.Core.Dtos.Git;

public class GitPersonDto
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("username")]
    public string? UserName { get; set; }

    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("avatar_url")]
    public string? AvatarUrl { get; set; }

    [JsonProperty("web_url")]
    public string? WebUrl { get; set; }

    [JsonProperty("public_email")]
    public string? Email { get; set; }

    [JsonIgnore]
    public bool IsBot { get; set; }

    [JsonIgnore]
    public string DisplayName => !string.IsNullOrWhiteSpace(Name) ? Name! : UserName ?? "?";
}
