using QRCoder;

namespace Taskist.Web.Helpers.TwoFactor;

public static class TwoFactorQrCode
{
    #region Methods

    public static string ToPngDataUri(string content, int pixelsPerModule = 5)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.M);
        var png = new PngByteQRCode(data).GetGraphic(pixelsPerModule);

        return $"data:image/png;base64,{Convert.ToBase64String(png)}";
    }

    #endregion
}
