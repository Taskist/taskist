namespace Taskist.Web.Models.Common;

public class BasePageModel
{
    public BasePageModel()
    {
        BreadCrumb = new List<BreadCrumbModel>();
        AccessDenied = false;
    }

    public string PageTitle { get; set; }

    public List<BreadCrumbModel> BreadCrumb { get; set; }

    public bool AccessDenied { get; set; }
}