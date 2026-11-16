namespace Taskist.Web.Models.Masters;

public class EmailTemplateGridModel
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string EmailSubject { get; set; }

    public string EmailAccount { get; set; }

    public bool Active { get; set; }
}
