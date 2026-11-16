using Taskist.Core.Common;

namespace Taskist.Tests.Common;

public class UploadPolicyTests
{
    #region Attachments

    [Theory]
    [InlineData("report.pdf")]
    [InlineData("SCREENSHOT.PNG")]
    [InlineData("notes.txt")]
    [InlineData("spec.docx")]
    [InlineData("photo.jpeg")]
    public void IsAllowedAttachment_AcceptsListedTypes(string fileName)
    {
        Assert.True(UploadPolicy.IsAllowedAttachment(fileName, 1024));
    }

    [Theory]
    [InlineData("page.html")]
    [InlineData("vector.svg")]
    [InlineData("script.js")]
    [InlineData("setup.exe")]
    [InlineData("report.pdf.html")]
    [InlineData("no-extension")]
    [InlineData("")]
    [InlineData(null)]
    public void IsAllowedAttachment_RejectsOtherTypes(string? fileName)
    {
        Assert.False(UploadPolicy.IsAllowedAttachment(fileName, 1024));
    }

    [Fact]
    public void IsAllowedAttachment_EnforcesTheSizeLimit()
    {
        var limit = UploadPolicy.ToBytes(UploadPolicy.MaxAttachmentMegabytes);

        Assert.True(UploadPolicy.IsAllowedAttachment("a.pdf", limit));
        Assert.False(UploadPolicy.IsAllowedAttachment("a.pdf", limit + 1));
        Assert.False(UploadPolicy.IsAllowedAttachment("a.pdf", 0));
    }

    #endregion

    #region Images

    [Theory]
    [InlineData(".png", "image/png")]
    [InlineData(".JPG", "image/jpeg")]
    [InlineData(".jpeg", "image/jpeg")]
    [InlineData(".gif", "image/gif")]
    [InlineData(".webp", "image/webp")]
    public void ImageContentType_MapsRasterImages(string extension, string expected)
    {
        Assert.Equal(expected, UploadPolicy.ImageContentType(extension));
    }

    [Theory]
    [InlineData(".svg")]
    [InlineData(".html")]
    [InlineData(".pdf")]
    [InlineData("")]
    [InlineData(null)]
    public void ImageContentType_IsNullForAnythingThatIsNotARasterImage(string? extension)
    {
        Assert.Null(UploadPolicy.ImageContentType(extension));
    }

    [Fact]
    public void IsAllowedImage_ChecksTypeAndSize()
    {
        Assert.True(UploadPolicy.IsAllowedImage("avatar.jpg", 1024, 2));
        Assert.False(UploadPolicy.IsAllowedImage("avatar.svg", 1024, 2));
        Assert.False(UploadPolicy.IsAllowedImage("avatar.jpg", UploadPolicy.ToBytes(2) + 1, 2));
    }

    #endregion
}
