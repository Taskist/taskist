
namespace Taskist.Web.Models.Common;

public class BasePageModel
{
    public int Id { get; set; }

    public string PageTitle { get; set; }

    public List<BreadCrumbModel> BreadCrumb { get; set; } = [];
}