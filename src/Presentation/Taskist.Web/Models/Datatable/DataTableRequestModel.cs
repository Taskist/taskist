using Newtonsoft.Json;

namespace Taskist.Web.Models.Datatable;

public class DataTableRequestModel
{
    [JsonProperty("draw")]
    public int Draw { get; set; }

    [JsonProperty("start")]
    public int Start { get; set; }

    [JsonProperty("length")]
    public int Length { get; set; }

    [JsonProperty("sortColumn")]
    public int SortColumn { get; set; }
}