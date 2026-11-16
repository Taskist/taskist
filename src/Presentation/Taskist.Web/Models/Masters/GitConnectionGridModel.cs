namespace Taskist.Web.Models.Masters;

public class GitConnectionGridModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Provider { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = string.Empty;

    public string AuthMode { get; set; } = string.Empty;

    public string? ServiceAccount { get; set; }

    public bool OAuth { get; set; }

    public bool IsDefault { get; set; }

    public bool Active { get; set; }
}
