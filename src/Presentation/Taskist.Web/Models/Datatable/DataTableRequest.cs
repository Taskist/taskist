namespace Taskist.Web.Models.Datatable;

public class DataTableRequest
{
    public DataTableRequest()
    {
        Search = new SearchData();
        Order = [];
    }

    public int Draw { get; set; }

    public int Start { get; set; }

    public int Length { get; set; }

    public SearchData? Search { get; set; }

    public List<ColumnOrder>? Order { get; set; }

    public List<Column> Columns { get; set; }

    #region Computed

    public string SortColumn
    {
        get
        {
            var columName = string.Empty;

            if (Order != null && Order.Count > 0)
                columName = Columns[Order[0].Column].Data;

            return columName;
        }
    }

    public string SortDirection
    {
        get
        {
            var direction = string.Empty;

            if (Order != null && Order.Count > 0)
                direction = Order[0].Dir;

            return direction;
        }
    }

    public string SearchValue
    {
        get
        {
            var searchValue = string.Empty;

            if (Search != null)
                searchValue = Search.Value;

            return searchValue;
        }
    }

    #endregion

    public class ColumnOrder
    {
        public int Column { get; set; }

        public string Dir { get; set; }
    }

    public class SearchData
    {
        public string Value { get; set; }

        public bool Regex { get; set; }
    }

    public class Column
    {
        public string Data { get; set; }

        public string Name { get; set; }

        public bool Searchable { get; set; }

        public SearchData Search { get; set; }
    }
}