using Newtonsoft.Json;

namespace Taskist.Core.Dtos.Git;

public class BranchCommitDto
{
    [JsonProperty("committed_date")]
    public DateTime? CommittedDate { get; set; }

    [JsonProperty("author_name")]
    public string? AuthorName { get; set; }
}
