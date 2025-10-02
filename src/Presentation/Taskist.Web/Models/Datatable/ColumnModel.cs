using Taskist.Core.Common;

namespace Taskist.Web.Models.Datatable;

public class ColumnModel
{
    public ColumnModel()
    {
        Conditions = [];
        Sorting = true;
        Capitalize = false;
        MaxLength = 100;
        IsDate = false;
        IsRating = false;
        IsColor = false;
    }

    public string Title { get; set; }

    public string DataColumn { get; set; }

    public bool Sorting { get; set; }

    public bool Capitalize { get; set; }

    public int MaxLength { get; set; }

    public bool IsDate { get; set; }

    public bool IsRating { get; set; }

    public bool IsColor { get; set; }

    public bool IsIconClass { get; set; }

    public List<ColumnConditionModel> Conditions { get; set; }
}

public class ColumnConditionModel
{
    public ColumnConditionModel()
    {
        DataType = DataTypeEnum.STRING;
        HideIfTrue = false;
    }

    public string Value { get; set; }

    public OperatorEnum Operator { get; set; }

    public DataTypeEnum DataType { get; set; }

    public string TextColor { get; set; }

    public string BgColor { get; set; }

    public string Icon { get; set; }

    public string ReplaceWith { get; set; }

    public string ReferenceParameter { get; set; }

    public bool HideIfTrue { get; set; }
}

public class ActionModel
{
    public ActionModel()
    {
        VisibleConditions = [];
        DeleteConfirmBox = false;
    }

    public string Id { get; set; }

    public string Title { get; set; }

    public string TitlePrefixDataParam { get; set; }

    public string TitleSufixDataParam { get; set; }

    public string DisplayReferenceParameter { get; set; }

    public bool DeleteConfirmBox { get; set; }

    public string DeleteConfirmBoxMsg { get; set; }

    public bool ConfirmBox { get; set; }

    public string ConfirmBoxMsg { get; set; }

    public string Url { get; set; }

    public string ReferenceParameter { get; set; }

    public string HideReferenceParameter { get; set; }

    public string Text { get; set; }

    public string AuxText { get; set; }

    public string Color { get; set; }

    public string Icon { get; set; }

    public string ModalSize { get; set; }

    public ButtonColorEnum ButtonColor { get; set; }

    public string ButtonClass { get; set; }

    public NavigationTypeEnum NavigationType { get; set; }

    public HyperLinkTypeEnum HyperLinkType { get; set; }

    public List<ActionCoditionModel> VisibleConditions { get; set; }
}

public class ActionCoditionModel
{
    public ActionCoditionModel()
    {
        DataType = DataTypeEnum.STRING;
    }

    public string Value { get; set; }

    public OperatorEnum Operator { get; set; }

    public DataTypeEnum DataType { get; set; }

    public string ReferenceParameter { get; set; }
}