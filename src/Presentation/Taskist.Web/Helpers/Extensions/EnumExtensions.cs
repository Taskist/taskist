using Microsoft.AspNetCore.Mvc.Rendering;

namespace Taskist.Web.Helpers.Extensions;

public static class EnumExtensions
{
    public static IList<SelectListItem> ToSelectList<TEnum>(this TEnum value) where TEnum : struct, Enum
    {
        return Enum.GetValues<TEnum>()
            .Select(item => new SelectListItem
            {
                Text = item.ToString(),
                Value = Convert.ToInt32(item).ToString(),
                Selected = item.Equals(value)
            })
            .ToList();
    }
}
