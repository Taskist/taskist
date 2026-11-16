namespace Taskist.Core.Dtos.Reports;

public class ResourceLoadResultDto
{
    public int Free { get; set; }

    public int Light { get; set; }

    public int Balanced { get; set; }

    public int Overloaded { get; set; }

    public List<ResourceLoadDto> Rows { get; set; } = [];
}
