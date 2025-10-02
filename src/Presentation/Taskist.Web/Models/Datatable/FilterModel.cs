namespace Taskist.Web.Models.Datatable;

public class FilterModel
{
    /// <summary>
    /// Name of the parameter name that's passed to post request
    /// </summary>
    public string Parameter { get; set; }

    /// <summary>
    /// Name of the html element
    /// </summary>
    public string Element { get; set; }
}