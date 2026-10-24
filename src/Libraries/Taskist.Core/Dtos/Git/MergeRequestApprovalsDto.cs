using Newtonsoft.Json;

namespace Taskist.Core.Dtos.Git;

public class MergeRequestApprovalsDto
{
    [JsonProperty("approved_by")]
    public List<MergeRequestApproverDto> ApprovedBy { get; set; } = [];

    [JsonProperty("user_has_approved")]
    public bool UserHasApproved { get; set; }

    [JsonProperty("user_can_approve")]
    public bool UserCanApprove { get; set; }
}
