using System.Linq.Dynamic.Core.Exceptions;
using FluentAssertions;
using Taskist.Data.Extensions;
using Xunit;

namespace Taskist.Tests.Data;

public class SortingExtensionsTests
{
    #region Utilities

    private sealed class Widget
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int Rank { get; set; }

        public Maker Maker { get; set; } = new();
    }

    private sealed class Maker
    {
        public string Name { get; set; } = string.Empty;
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
    public void OrderBySafe_SortsAscendingByDefault()
    {
        var result = Widgets().OrderBySafe("Name", "asc").ToList();

        result.Select(x => x.Name).Should().ContainInOrder("apple", "banana", "cherry");
    }

    [Fact]
    public void OrderBySafe_SortsDescending()
    {
        var result = Widgets().OrderBySafe("Rank", "desc").ToList();

        result.Select(x => x.Rank).Should().ContainInOrder(3, 2, 1);
    }

    [Fact]
    public void OrderBySafe_AcceptsColumnNamesInAnyCase()
    {
        //DataTables sends the client side field name, which may differ in casing
        var result = Widgets().OrderBySafe("nAmE", "asc").ToList();

        result.Select(x => x.Name).Should().ContainInOrder("apple", "banana", "cherry");
    }

    [Fact]
    public void OrderBySafe_SupportsNestedProperties()
    {
        var result = Widgets().OrderBySafe("Maker.Name", "asc").ToList();

        result.Select(x => x.Maker.Name).Should().ContainInOrder("alpha", "mid", "zeta");
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
    public void OrderBySafe_IgnoresAnythingThatIsNotAProperty(string malicious)
    {
        //unknown input must fall back to the default rather than reach Dynamic LINQ
        var act = () => Widgets().OrderBySafe(malicious, "asc").ToList();

        act.Should().NotThrow<ParseException>();
        act().Select(x => x.Id).Should().ContainInOrder(1, 2, 3);
    }

    [Fact]
    public void OrderBySafe_IgnoresAnInjectedSortDirection()
    {
        var result = Widgets().OrderBySafe("Name", "asc; DROP TABLE Widget").ToList();

        //an unrecognised direction is treated as ascending
        result.Select(x => x.Name).Should().ContainInOrder("apple", "banana", "cherry");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void OrderBySafe_WithNoColumn_FallsBackToTheDefault(string? column)
    {
        var result = Widgets().OrderBySafe(column!, "asc").ToList();

        result.Select(x => x.Id).Should().ContainInOrder(1, 2, 3);
    }

    [Fact]
    public void OrderBySafe_WithAnUnknownDefault_LeavesTheQueryUnordered()
    {
        var act = () => Widgets().OrderBySafe("bogus", "asc", "alsoBogus").ToList();

        act.Should().NotThrow();
        act().Should().HaveCount(3);
    }

    #endregion

    #region Multi column overload

    [Fact]
    public void OrderBySafe_OrdersBySeveralColumns()
    {
        var result = Widgets().OrderBySafe(["Maker.Name", "Name"]).ToList();

        result.Select(x => x.Maker.Name).Should().ContainInOrder("alpha", "mid", "zeta");
    }

    [Fact]
    public void OrderBySafe_DropsUnknownColumnsFromTheList()
    {
        var result = Widgets().OrderBySafe(["Name", "DROP TABLE Widget"]).ToList();

        result.Select(x => x.Name).Should().ContainInOrder("apple", "banana", "cherry");
    }

    [Fact]
    public void OrderBySafe_WithNoUsableColumns_LeavesTheQueryUnordered()
    {
        var act = () => Widgets().OrderBySafe(["nope", "also nope"]).ToList();

        act.Should().NotThrow();
        act().Should().HaveCount(3);
    }

    [Fact]
    public void OrderBySafe_WithNullList_LeavesTheQueryUnordered()
    {
        var act = () => Widgets().OrderBySafe(null!).ToList();

        act.Should().NotThrow();
        act().Should().HaveCount(3);
    }

    #endregion
}
