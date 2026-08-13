namespace Taskist.Web.Helpers.Common;

/// <summary>
/// Content based checks for uploaded images. The file name and the browser
/// supplied content type are both attacker controlled, so the bytes decide.
/// </summary>
public static class ImageHelper
{
    #region Field

    /// <summary>
    /// Leading bytes that identify the image formats accepted for avatars.
    /// </summary>
    private static readonly byte[][] _signatures =
    [
        [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A],   //PNG
        [0xFF, 0xD8, 0xFF],                                 //JPEG
        [0x47, 0x49, 0x46, 0x38],                           //GIF87a / GIF89a
        [0x52, 0x49, 0x46, 0x46]                            //RIFF container, WebP
    ];

    #endregion

    #region Methods

    /// <summary>
    /// Indicates whether the upload starts with a recognised image signature.
    /// </summary>
    public static bool IsImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return false;

        try
        {
            using var stream = file.OpenReadStream();

            var header = new byte[8];
            var read = stream.Read(header, 0, header.Length);

            if (read < 3)
                return false;

            return _signatures.Any(signature =>
                read >= signature.Length &&
                header.Take(signature.Length).SequenceEqual(signature));
        }
        catch (IOException)
        {
            return false;
        }
    }

    #endregion
}
