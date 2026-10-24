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