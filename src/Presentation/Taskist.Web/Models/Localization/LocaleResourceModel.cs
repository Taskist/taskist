using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Common;

namespace Taskist.Web.Models.Localization;

public class LocaleResourceModel : BaseModel
{
    public int LanguageId { get; set; }

    [LocalizedDisplayName("LocaleResourceModel.ResourceKey")]
    public string ResourceKey { get; set; }

    [LocalizedDisplayName("LocaleResourceModel.ResourceValue")]
    public string ResourceValue { get; set; }
}
