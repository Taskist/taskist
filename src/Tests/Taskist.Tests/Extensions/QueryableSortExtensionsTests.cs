using Taskist.Core.Extensions;

namespace Taskist.Tests.Extensions;

public class QueryableSortExtensionsTests
{
    #region Utilities

    private sealed class Widget
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int Rank { get; set; }

        public Maker Maker { get; set; } = new();

        public List<Maker> Suppliers { get; set; } = [];
    }

    private sealed class Maker
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class Opaque
    {
        public List<Maker> Makers { get; set; } = [];
    }

    private static IQueryable<Widget> Widgets() => new List<Widget>
    {
        new() { Id = 3, Name = "cherry", Rank = 1, Maker = new Maker { Name = "zeta" } },
        new() { Id = 1, Name = "apple",  Rank = 3, Maker = new Maker { Name = "alpha" } },
        new() { Id = 2, Name = "banana", Rank = 2, Maker = new Maker { Name = "mid" } }
    }.AsQueryable();

    #endregion

    #region Valid input

    [Fact]
    public void ApplySafeSort_SortsAscendingByDefault()
    {
        var result = Widgets().ApplySafeSort("Name", null).Select(x => x.Name).ToList();

        Assert.Equal(["apple", "banana", "cherry"], result);
    }

    [Fact]
    public void ApplySafeSort_SortsDescending()
    {
        var result = Widgets().ApplySafeSort("Rank", "DESC").Select(x => x.Rank).ToList();

        Assert.Equal([3, 2, 1], result);
    }

    [Fact]
    public void ApplySafeSort_AcceptsColumnNamesInAnyCase()
    {
        var result = Widgets().ApplySafeSort("  nAmE ", "asc").Select(x => x.Name).ToList();

        Assert.Equal(["apple", "banana", "cherry"], result);
    }

    [Fact]
    public void ApplySafeSort_SupportsOneLevelNavigationProperties()
    {
        var result = Widgets().ApplySafeSort("maker.name", "asc").Select(x => x.Maker.Name).ToList();

        Assert.Equal(["alpha", "mid", "zeta"], result);
    }

    #endregion

    #region Rejected input

    [Theory]
    [InlineData("DROP TABLE Widget")]
    [InlineData("Name; DELETE FROM Widget")]
    [InlineData("it.GetType()")]
    [InlineData("Name.Length.ToString()")]
    [InlineData("1 == 1 ? Name : Name")]
    [InlineData("NotARealProperty")]
    [InlineData("Maker.NotARealProperty")]
    [InlineData("Suppliers.Name")]
    public void ApplySafeSort_FallsBackToTheDefaultForAnythingNotAllowListed(string column)
    {
        var result = Widgets().ApplySafeSort(column, "asc").Select(x => x.Id).ToList();

        Assert.Equal([1, 2, 3], result);
    }

    [Fact]
    public void ApplySafeSort_TreatsAnInjectedDirectionAsAscending()
    {
        var result = Widgets().ApplySafeSort("Name", "desc; DROP TABLE Widget").Select(x => x.Name).ToList();

        Assert.Equal(["apple", "banana", "cherry"], result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ApplySafeSort_WithNoColumn_UsesTheDefault(string? column)
    {
        var result = Widgets().ApplySafeSort(column, "desc").Select(x => x.Id).ToList();

        Assert.Equal([3, 2, 1], result);
    }

    [Fact]
    public void ApplySafeSort_WithAnUnknownDefault_UsesTheFirstSortableProperty()
    {
        var result = Widgets().ApplySafeSort("bogus", "asc", "alsoBogus").Select(x => x.Id).ToList();

        Assert.Equal([1, 2, 3], result);
    }

    [Fact]
    public void ApplySafeSort_OnATypeWithoutSortableProperties_LeavesTheQueryAlone()
    {
        var items = new List<Opaque> { new(), new() }.AsQueryable();

        Assert.Equal(2, items.ApplySafeSort("Makers", "asc").Count());
    }

    #endregion
}
