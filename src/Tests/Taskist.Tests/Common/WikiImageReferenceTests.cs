using Taskist.Service.Wiki;

namespace Taskist.Tests.Common;

public class WikiImageReferenceTests
{
    #region Tests

    [Theory]
    [InlineData("/WikiDocument/Image/12", 12)]
    [InlineData("WikiDocument/Image/12", 12)]
    [InlineData("../WikiDocument/Image/12", 12)]
    [InlineData("https://taskist.example.com/WikiDocument/Image/12", 12)]
    [InlineData("/WikiDocument/Image?id=12", 12)]
    [InlineData("/wikidocument/image/12", 12)]
    public void ExtractId_ReadsBothUrlForms(string url, int expected)
    {
        Assert.Equal(expected, WikiImageReference.ExtractId(url));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("/Backlog/ViewDocument/12")]
    [InlineData("/WikiDocument/Image/")]
    [InlineData("/WikiDocument/Image/abc")]
    [InlineData("/WikiDocument/Image/0")]
    public void ExtractId_ReturnsNullForOtherUrls(string? url)
    {
        Assert.Null(WikiImageReference.ExtractId(url));
    }

    [Fact]
    public void References_MatchesTheExactIdOnly()
    {
        const string html = "<p><img src=\"/WikiDocument/Image/123\"></p><p><img src=\"/WikiDocument/Image?id=45\"></p>";

        Assert.True(WikiImageReference.References(html, 123));
        Assert.True(WikiImageReference.References(html, 45));
        Assert.False(WikiImageReference.References(html, 12));
        Assert.False(WikiImageReference.References(html, 4));
        Assert.False(WikiImageReference.References(html, 1234));
    }

    [Fact]
    public void References_IsFalseForEmptyContent()
    {
        Assert.False(WikiImageReference.References(null, 1));
        Assert.False(WikiImageReference.References("<p>no images</p>", 1));
    }

    [Fact]
    public void SearchFragments_CoverBothUrlForms()
    {
        Assert.Equal(["WikiDocument/Image/7", "WikiDocument/Image?id=7"], WikiImageReference.SearchFragments(7));
    }

    #endregion
}
