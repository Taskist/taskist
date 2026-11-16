using Taskist.Web.Helpers.Common;

namespace Taskist.Tests.Common;

public class WikiNodeIdTests
{
    #region Tests

    [Theory]
    [InlineData("page_12", 12)]
    [InlineData("12", 12)]
    [InlineData(" page_7 ", 7)]
    public void TryParsePage_AcceptsPrefixedAndPlainIds(string value, int expected)
    {
        Assert.True(WikiNodeId.TryParsePage(value, out var id));
        Assert.Equal(expected, id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("page_")]
    [InlineData("page_abc")]
    [InlineData("page_-3")]
    [InlineData("page_0")]
    [InlineData("page_1.5")]
    [InlineData("page_99999999999")]
    [InlineData("cat_4")]
    public void TryParsePage_RejectsMalformedIds(string? value)
    {
        Assert.False(WikiNodeId.TryParsePage(value, out var id));
        Assert.Equal(0, id);
    }

    [Fact]
    public void TryParseCategory_UsesTheCategoryPrefix()
    {
        Assert.True(WikiNodeId.TryParseCategory("cat_4", out var id));
        Assert.Equal(4, id);
        Assert.False(WikiNodeId.TryParseCategory("page_4", out _));
    }

    #endregion
}
