namespace Taskist.Core.Common;

public static class UploadPolicy
{
    #region Fields

    public const int MaxAttachmentMegabytes = 2;

    public const int MaxImageMegabytes = 5;

    public const int MaxProfilePhotoMegabytes = 2;

    public static readonly IReadOnlySet<string> AttachmentExtensions =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".docx", ".txt" };

    private static readonly IReadOnlyDictionary<string, string> ImageContentTypes =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".gif"] = "image/gif",
            [".webp"] = "image/webp",
            [".bmp"] = "image/bmp"
        };

    #endregion

    #region Methods

    public static long ToBytes(int megabytes) => megabytes * 1024L * 1024L;

    public static bool IsAllowedAttachment(string? fileName, long length)
    {
        return length > 0
            && length <= ToBytes(MaxAttachmentMegabytes)
            && AttachmentExtensions.Contains(Path.GetExtension(fileName ?? string.Empty));
    }

    public static bool IsAllowedImage(string? fileName, long length, int maxMegabytes)
    {
        return length > 0
            && length <= ToBytes(maxMegabytes)
            && ImageContentType(Path.GetExtension(fileName ?? string.Empty)) != null;
    }

    public static string? ImageContentType(string? extension)
    {
        return extension != null && ImageContentTypes.TryGetValue(extension, out var contentType) ? contentType : null;
    }

    public static string AcceptAttribute() => string.Join(",", AttachmentExtensions);

    #endregion
}
