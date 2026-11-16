using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;
using Taskist.Core.Enums;
using Taskist.Web.Models.Masters;

namespace Taskist.Tests.WorkItems;

public class CustomFieldLayoutTests
{
    #region Utilities

    private static CustomField Field(int id, int placement, int? slot = null, int? order = null)
    {
        return new CustomField { Id = id, Placement = placement, Slot = slot, DisplayOrder = order };
    }

    private const int Main = (int)CustomFieldPlacement.MainSection;
    private const int Status = (int)CustomFieldPlacement.StatusSection;

    #endregion

    #region ResolveSlot

    [Fact]
    public void ResolveSlot_LegacyMainField_GoesToMainBottom()
    {
        Assert.Equal(BacklogFormSlot.MainBottom, CustomFieldLayout.ResolveSlot(null, Main));
    }

    [Fact]
    public void ResolveSlot_LegacyStatusField_GoesToStatusBottom()
    {
        Assert.Equal(BacklogFormSlot.StatusBottom, CustomFieldLayout.ResolveSlot(null, Status));
    }

    [Fact]
    public void ResolveSlot_ExplicitSlot_WinsOverPlacement()
    {
        Assert.Equal(BacklogFormSlot.StatusAfterStatus,
            CustomFieldLayout.ResolveSlot((int)BacklogFormSlot.StatusAfterStatus, Main));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(999)]
    [InlineData(-5)]
    public void ResolveSlot_UnknownSlot_FallsBackToPlacementBottom(int slot)
    {
        Assert.Equal(BacklogFormSlot.StatusBottom, CustomFieldLayout.ResolveSlot(slot, Status));
        Assert.Equal(BacklogFormSlot.MainBottom, CustomFieldLayout.ResolveSlot(slot, Main));
    }

    [Fact]
    public void ResolveSlot_UnknownPlacement_GoesToMainBottom()
    {
        Assert.Equal(BacklogFormSlot.MainBottom, CustomFieldLayout.ResolveSlot(null, 0));
    }

    #endregion

    #region Sections

    [Fact]
    public void SectionOf_EverySlot_MatchesItsPrefix()
    {
        foreach (var slot in Enum.GetValues<BacklogFormSlot>())
        {
            var expected = slot.ToString().StartsWith("Status")
                ? CustomFieldPlacement.StatusSection
                : CustomFieldPlacement.MainSection;
            Assert.Equal(expected, CustomFieldLayout.SectionOf(slot));
        }
    }

    [Fact]
    public void SlotsIn_CoverEverySlotOnceInFormOrder()
    {
        var main = CustomFieldLayout.SlotsIn(CustomFieldPlacement.MainSection);
        var status = CustomFieldLayout.SlotsIn(CustomFieldPlacement.StatusSection);

        Assert.Equal(Enum.GetValues<BacklogFormSlot>().Length, main.Count + status.Count);
        Assert.Equal(BacklogFormSlot.MainAfterTitle, main[0]);
        Assert.Equal(BacklogFormSlot.MainBottom, main[^1]);
        Assert.Equal(BacklogFormSlot.StatusAfterProject, status[0]);
        Assert.Equal(BacklogFormSlot.StatusBottom, status[^1]);
    }

    [Fact]
    public void RendersInsideStatusRow_OnlyForStatusSlotsAboveTheBottom()
    {
        Assert.True(CustomFieldLayout.RendersInsideStatusRow(BacklogFormSlot.StatusAfterProject));
        Assert.True(CustomFieldLayout.RendersInsideStatusRow(BacklogFormSlot.StatusAfterRelease));
        Assert.False(CustomFieldLayout.RendersInsideStatusRow(BacklogFormSlot.StatusBottom));
        Assert.False(CustomFieldLayout.RendersInsideStatusRow(BacklogFormSlot.MainAfterTitle));
        Assert.False(CustomFieldLayout.RendersInsideStatusRow(BacklogFormSlot.MainBottom));
    }

    #endregion

    #region Order

    [Fact]
    public void Order_SortsBySlotThenDisplayOrderThenId()
    {
        var fields = new[]
        {
            Field(1, Status),
            Field(2, Main, (int)BacklogFormSlot.MainAfterTitle, 5),
            Field(3, Main, (int)BacklogFormSlot.MainAfterTitle, 1),
            Field(4, Status, (int)BacklogFormSlot.StatusAfterStatus),
            Field(5, Main),
            Field(6, Main, (int)BacklogFormSlot.MainAfterTitle)
        };

        var ordered = CustomFieldLayout.Order(fields).Select(x => x.Id).ToArray();

        Assert.Equal([3, 2, 6, 5, 4, 1], ordered);
    }

    [Fact]
    public void Order_NumberedFieldsComeBeforeUnnumbered()
    {
        var fields = new[]
        {
            Field(1, Main),
            Field(2, Main, null, 0),
            Field(3, Main, null, 9999)
        };

        Assert.Equal([2, 3, 1], CustomFieldLayout.Order(fields).Select(x => x.Id).ToArray());
    }

    [Fact]
    public void Order_NullInputAndNullItems_AreIgnored()
    {
        Assert.Empty(CustomFieldLayout.Order(null!));
        Assert.Single(CustomFieldLayout.Order([null!, Field(1, Main)]));
    }

    #endregion

    #region Layout model

    [Fact]
    public void LayoutFor_ReturnsOnlyTheSlotsFieldsAndRowMode()
    {
        var layout = new CustomFieldLayoutModel
        {
            CanUpdate = true,
            Fields =
            [
                new CustomFieldRenderModel { Id = 1, Index = 0, ResolvedSlot = (int)BacklogFormSlot.MainAfterTitle },
                new CustomFieldRenderModel { Id = 2, Index = 1, ResolvedSlot = (int)BacklogFormSlot.StatusAfterStatus },
                new CustomFieldRenderModel { Id = 3, Index = 2, ResolvedSlot = (int)BacklogFormSlot.StatusAfterStatus }
            ]
        };

        var status = layout.For(BacklogFormSlot.StatusAfterStatus);
        var main = layout.For(BacklogFormSlot.MainAfterTitle);
        var empty = layout.For(BacklogFormSlot.StatusBottom);

        Assert.Equal([2, 3], status.CustomFields.Select(x => x.Id).ToArray());
        Assert.True(status.InsideRow);
        Assert.True(status.CanUpdate);
        Assert.Single(main.CustomFields);
        Assert.False(main.InsideRow);
        Assert.Empty(empty.CustomFields);
    }

    #endregion
}
