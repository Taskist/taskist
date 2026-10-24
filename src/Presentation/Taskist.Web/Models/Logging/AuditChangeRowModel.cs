namespace Taskist.Web.Models.Logging;

public class AuditChangeRowModel
{
    public string Field { get; set; }

    public string Old { get; set; }

    public string New { get; set; }

    public bool Changed { get; set; }
}
