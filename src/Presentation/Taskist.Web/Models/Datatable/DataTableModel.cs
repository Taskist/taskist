namespace Taskist.Web.Models.Datatable;

public class DataTableModel
{
    public DataTableModel()
    {
        Columns = [];
        HeaderActions = [];
        ColumnActions = [];
        FilterParameters = [];
        RenderOnlyTable = false;
        ServerSide = true;
        StateSave = true;
        LengthChange = true;
        Searching = true;
        Sorting = true;
        LengthChangeLabel = "Show:";
        SearchLabel = "Filter:";
        SearchPlaceholder = "Type to filter...";
        ZeroRecordsLabel = "Nothing found";
        FooterInfoLabel = "Showing page _PAGE_ of _PAGES_";
        FooterInfoWhenEmptyLabel = "No records available";
        AddButtonToHeader = false;
    }

    public string Title { get; set; }

    public string Name { get; set; }

    public string UrlRead { get; set; }

    public List<ActionModel> HeaderActions { get; set; }

    public List<ActionModel> ColumnActions { get; set; }

    public IList<FilterModel> FilterParameters { get; set; }

    public string ReferenceColumn { get; set; }

    public bool RenderOnlyTable { get; set; }

    public bool ServerSide { get; set; }

    public bool StateSave { get; set; }

    public bool LengthChange { get; set; }

    public bool Searching { get; set; }

    public bool Sorting { get; set; }

    public string LengthChangeLabel { get; set; }

    public string SearchLabel { get; set; }

    public string SearchPlaceholder { get; set; }

    public string ZeroRecordsLabel { get; set; }

    public string FooterInfoLabel { get; set; }

    public string FooterInfoWhenEmptyLabel { get; set; }

    public bool AddButtonToHeader { get; set; }

    public IList<ColumnModel> Columns { get; set; }
}