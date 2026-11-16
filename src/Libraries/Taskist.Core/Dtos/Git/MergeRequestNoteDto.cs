using Newtonsoft.Json;

namespace Taskist.Core.Dtos.Git;

public class MergeRequestNoteDto
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("body")]
    public string? Body { get; set; }

    [JsonProperty("author")]
    public GitPersonDto? Author { get; set; }

    [JsonProperty("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("system")]
    public bool System { get; set; }
}
