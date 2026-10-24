using Newtonsoft.Json;

namespace Taskist.Core.Dtos.Git;

public class MergeRequestApproverDto
{
    [JsonProperty("user")]
    public GitPersonDto? User { get; set; }
}
