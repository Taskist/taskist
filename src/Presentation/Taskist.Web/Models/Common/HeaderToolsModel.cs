namespace Taskist.Web.Models.Common;

/// <summary>
/// Backs the top-bar tools cluster: global search + the quick-add ("+") menu.
/// Every flag is permission-gated so the UI only offers what the user may do.
/// </summary>
public class HeaderToolsModel
{
    // which part to render: "search", "create", or "all"
    public string Section { get; set; } = "all";

    public bool ShowSearch => (Section == "all" || Section == "search") && CanSearch;
    public bool ShowCreate => (Section == "all" || Section == "create") && HasCreate;

    // global search is available to anyone who can reach the dashboard
    public bool CanSearch { get; set; }

    // quick-add ("+") menu
    public bool CanCreateTask { get; set; }
    public bool CanCreateProject { get; set; }
    public bool CanCreateClient { get; set; }

    // convenience: is there anything to show under the "+" at all
    public bool HasCreate => CanCreateTask || CanCreateProject || CanCreateClient;
}
