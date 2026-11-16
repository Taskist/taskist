using Taskist.Web.Helpers.Wiki;

namespace Taskist.Tests.Common;

public class WikiTreeRulesTests
{
    #region Fields

    private static readonly IReadOnlyDictionary<int, int?> Tree = new Dictionary<int, int?>
    {
        [1] = null,
        [2] = 1,
        [3] = 2,
        [4] = null,
        [5] = 4
    };

    #endregion

    #region Tests

    [Theory]
    [InlineData(2, 2)]
    [InlineData(1, 2)]
    [InlineData(1, 3)]
    [InlineData(2, 3)]
    public void WouldCreateCycle_WhenMovingIntoItselfOrADescendant(int moving, int newParent)
    {
        Assert.True(WikiTreeRules.WouldCreateCycle(moving, newParent, Tree));
    }

    [Theory]
    [InlineData(3, null)]
    [InlineData(3, 1)]
    [InlineData(2, 4)]
    [InlineData(5, 3)]
    public void WouldNotCreateCycle_ForTopLevelSiblingOrOtherBranch(int moving, int? newParent)
    {
        Assert.False(WikiTreeRules.WouldCreateCycle(moving, newParent, Tree));
    }

    [Fact]
    public void WouldCreateCycle_StopsOnAnAlreadyBrokenLoop()
    {
        var broken = new Dictionary<int, int?> { [10] = 11, [11] = 10 };

        Assert.True(WikiTreeRules.WouldCreateCycle(99, 10, broken));
    }

    [Theory]
    [InlineData(2, 2, true)]
    [InlineData(2, 3, false)]
    [InlineData(null, 2, false)]
    [InlineData(2, null, false)]
    [InlineData(0, 0, false)]
    public void IsSameProject_RequiresTwoEqualRealProjects(int? source, int? target, bool expected)
    {
        Assert.Equal(expected, WikiTreeRules.IsSameProject(source, target));
    }

    #endregion
}
