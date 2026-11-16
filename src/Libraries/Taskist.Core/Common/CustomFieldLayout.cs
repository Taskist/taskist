using Taskist.Core.Domain.Masters;
using Taskist.Core.Enums;

namespace Taskist.Core.Common;

public static class CustomFieldLayout
{
    #region Fields

    private const int StatusSectionStart = 100;

    #endregion

    #region Methods

    public static BacklogFormSlot ResolveSlot(int? slot, int placement)
    {
        if (slot.HasValue && Enum.IsDefined(typeof(BacklogFormSlot), slot.Value))
            return (BacklogFormSlot)slot.Value;

        return placement == (int)CustomFieldPlacement.StatusSection
            ? BacklogFormSlot.StatusBottom
            : BacklogFormSlot.MainBottom;
    }

    public static CustomFieldPlacement SectionOf(BacklogFormSlot slot)
    {
        return (int)slot >= StatusSectionStart
            ? CustomFieldPlacement.StatusSection
            : CustomFieldPlacement.MainSection;
    }

    public static bool RendersInsideStatusRow(BacklogFormSlot slot)
    {
        return SectionOf(slot) == CustomFieldPlacement.StatusSection && slot != BacklogFormSlot.StatusBottom;
    }

    public static IList<CustomField> Order(IEnumerable<CustomField> fields)
    {
        return (fields ?? [])
            .Where(x => x != null)
            .OrderBy(x => (int)ResolveSlot(x.Slot, x.Placement))
            .ThenBy(x => x.DisplayOrder ?? int.MaxValue)
            .ThenBy(x => x.Id)
            .ToList();
    }

    public static IReadOnlyList<BacklogFormSlot> SlotsIn(CustomFieldPlacement section)
    {
        return Enum.GetValues<BacklogFormSlot>()
            .Where(x => SectionOf(x) == section)
            .OrderBy(x => (int)x)
            .ToList();
    }

    #endregion
}
