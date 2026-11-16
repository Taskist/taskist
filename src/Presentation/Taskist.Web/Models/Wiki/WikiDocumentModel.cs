using Microsoft.AspNetCore.Mvc.Rendering;

namespace Taskist.Web.Models.Wiki;

public class WikiDocumentModel
{
    public WikiDocumentModel()
    {
        AvailableProjects = [];
    }

    public bool HasProject { get; set; }

    public bool CanAddWiki { get; set; }

    public int SelectedProjectId { get; set; }

    public IList<SelectListItem> AvailableProjects { get; set; }
}
