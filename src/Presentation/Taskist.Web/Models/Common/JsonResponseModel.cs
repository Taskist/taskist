using Newtonsoft.Json;
using Taskist.Core.Common;

namespace Taskist.Web.Models.Common;

public class JsonResponseModel
{
    public JsonResponseModel()
    {
        Errors = [];
        ChildAction = false;
    }

    [JsonProperty("status")]
    public HttpStatusCodeEnum Status { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }

    [JsonProperty("errors")]
    public List<ErrorModel> Errors { get; set; }

    [JsonProperty("child_action")]
    public bool ChildAction { get; set; }
}
