namespace Taskist.Web.Models.Logging;

public class AuditDetailModel
{
    public string Entity { get; set; }

    public int? EntityId { get; set; }

    public string Action { get; set; }

    public string User { get; set; }

    public DateTime When { get; set; }

    public string Ip { get; set; }

    public List<AuditChangeRowModel> Changes { get; set; } = [];

    public string OldRaw { get; set; }

    public string NewRaw { get; set; }
}
