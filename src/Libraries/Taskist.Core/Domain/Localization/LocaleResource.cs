using Taskist.Core.Domain.Common;

namespace Taskist.Core.Domain.Localization;

public class LocaleResource : BaseEntity
{
    public int LanguageId { get; set; }

    public string ResourceKey { get; set; }

    public string ResourceValue { get; set; }

    public virtual Language Language { get; set; }
}
