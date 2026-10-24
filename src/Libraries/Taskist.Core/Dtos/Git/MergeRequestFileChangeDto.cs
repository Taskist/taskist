using Newtonsoft.Json;

namespace Taskist.Core.Dtos.Git;

public class MergeRequestFileChangeDto
{
    [JsonProperty("old_path")]
    public string? OldPath { get; set; }

    [JsonProperty("new_path")]
    public string? NewPath { get; set; }

    [JsonProperty("new_file")]
    public bool NewFile { get; set; }

    [JsonProperty("renamed_file")]
    public bool RenamedFile { get; set; }

    [JsonProperty("deleted_file")]
    public bool DeletedFile { get; set; }

    [JsonProperty("diff")]
    public string? Diff { get; set; }

    [JsonProperty("too_large")]
    public bool TooLarge { get; set; }
}
