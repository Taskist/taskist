using Microsoft.AspNetCore.DataProtection;
using Taskist.Web.Helpers.Documents;

namespace Taskist.Tests.Infrastructure;

public class UploadTokenServiceTests
{
    #region Utilities

    private static UploadTokenService CreateService()
    {
        return new UploadTokenService(new EphemeralDataProtectionProvider());
    }

    #endregion

    #region Tokens

    [Fact]
    public void Read_ReturnsTheDocumentForTheUploader()
    {
        var service = CreateService();

        Assert.Equal(42, service.Read(service.Create(7, 42), 7));
    }

    [Fact]
    public void Read_RejectsATokenIssuedToAnotherUser()
    {
        var service = CreateService();

        Assert.Null(service.Read(service.Create(7, 42), 8));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("42")]
    [InlineData("not-a-token")]
    public void Read_RejectsRawIdsAndGarbage(string? token)
    {
        Assert.Null(CreateService().Read(token, 7));
    }

    [Fact]
    public void Read_RejectsATokenFromAnotherKeyRing()
    {
        var token = CreateService().Create(7, 42);

        Assert.Null(CreateService().Read(token, 7));
    }

    [Fact]
    public void Read_RejectsATamperedToken()
    {
        var service = CreateService();
        var token = service.Create(7, 42);

        Assert.Null(service.Read(token[..^3] + "AAA", 7));
    }

    #endregion
}
