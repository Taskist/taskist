using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Taskist.Core.Domain.Users;

namespace Taskist.Service.Files;

public class FileStorageService : IFileStorageService
{
    #region Fields

    protected readonly IWebHostEnvironment _env;

    #endregion

    #region Ctor

    public FileStorageService(IWebHostEnvironment env)
    {
        _env = env;
    }

    #endregion

    #region Methods

    public async Task<byte[]?> GetFileAsync(string fileName, string folder)
    {
        if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(folder))
            return null;

        string fullPath = Path.Combine(_env.WebRootPath, folder, fileName);

        if (!File.Exists(fullPath))
            return null;

        return await File.ReadAllBytesAsync(fullPath);
    }

    public async Task<(byte[]? FileBytes, DateTime? LastModified)> GetAvatarFileAsync(User user)
    {
        var folder = "uploads\\avatars";
        var fileName = $"{user.Code}.png";

        if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(folder))
            return (null, null);

        string fullPath = Path.Combine(_env.WebRootPath, folder, fileName);

        if (!File.Exists(fullPath))
        {
            var defaultAvatar = "avatar.png";
            switch (user.GenderId)
            {
                case 1:
                    defaultAvatar = "avatar_male.png";
                    break;
                case 2:
                    defaultAvatar = "avatar_female.png";
                    break;
            }
            fullPath = Path.Combine(_env.WebRootPath, "images", defaultAvatar);
        }

        var bytes = await File.ReadAllBytesAsync(fullPath);
        var lastModified = File.GetLastWriteTimeUtc(fullPath);

        return (bytes, lastModified);
    }

    public async Task SaveFileAsync(IFormFile file, string folder, string? fileName = null)
    {
        var uploads = Path.Combine(_env.WebRootPath, folder);
        Directory.CreateDirectory(uploads);

        fileName ??= $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

        var filePath = Path.Combine(uploads, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        File.SetLastWriteTimeUtc(filePath, DateTime.UtcNow);
    }

    public Task DeleteFileAsync(string fileName, string folder)
    {
        var filePath = Path.Combine(_env.WebRootPath, folder, fileName);
        if (File.Exists(filePath))
            File.Delete(filePath);

        return Task.CompletedTask;
    }

    #endregion
}
