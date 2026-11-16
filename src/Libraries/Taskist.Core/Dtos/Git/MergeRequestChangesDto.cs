using Newtonsoft.Json;

namespace Taskist.Core.Dtos.Git;

public class MergeRequestChangesDto
{
    [JsonProperty("changes")]
    public List<MergeRequestFileChangeDto> Changes { get; set; } = [];

    [JsonProperty("overflow")]
    public bool Overflow { get; set; }

    [JsonProperty("web_url")]
    public string? WebUrl { get; set; }
}
