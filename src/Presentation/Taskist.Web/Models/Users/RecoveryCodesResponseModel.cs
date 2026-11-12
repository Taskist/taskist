using Newtonsoft.Json;
using Taskist.Web.Models.Common;

namespace Taskist.Web.Models.Users;

public class RecoveryCodesResponseModel : JsonResponseModel
{
    #region Properties

    [JsonProperty("codes")]
    public IReadOnlyList<string> Codes { get; set; } = [];

    #endregion
}
