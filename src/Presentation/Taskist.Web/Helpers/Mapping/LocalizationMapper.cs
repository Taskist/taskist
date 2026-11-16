using Riok.Mapperly.Abstractions;
using Taskist.Core.Domain.Localization;
using Taskist.Web.Models.Localization;

namespace Taskist.Web.Helpers.Mapping;

[Mapper]
public partial class LocalizationMapper
{
    public partial LanguageModel ToModel(Language source);
    public partial Language ToEntity(LanguageModel source);
    public partial void Update(LanguageModel source, Language target);

    public partial LocaleResourceModel ToModel(LocaleResource source);
    public partial LocaleResource ToEntity(LocaleResourceModel source);
    public partial void Update(LocaleResourceModel source, LocaleResource target);
}
