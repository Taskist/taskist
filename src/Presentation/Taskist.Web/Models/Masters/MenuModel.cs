using Taskist.Web.Models.Localization;

namespace Taskist.Web.Models.Masters;

public class MenuModel
{
    public MenuModel()
    {
        MenuItems = [];
        LocaleResources = [];
    }

    public List<MenuItemModel> MenuItems { get; set; }

    public List<LocaleResourceModel> LocaleResources { get; set; }
}

public class MenuItemModel
{
    public string Name { get; set; }

    public string SystemName { get; set; }

    public int Code { get; set; }

    public string ActionName { get; set; }

    public string ControllerName { get; set; }

    public int? ParentCode { get; set; }

    public string Icon { get; set; }

    public int DisplayOrder { get; set; }

    public string Permission { get; set; }
}