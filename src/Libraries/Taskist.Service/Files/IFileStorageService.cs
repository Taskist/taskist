using Microsoft.AspNetCore.Http;
using Taskist.Core.Domain.Users;

namespace Taskist.Service.Files;

public interface IFileStorageService
{
    Task<byte[]?> GetFileAsync(string fileName, string folder);

    Task<(byte[]? FileBytes, DateTime? LastModified)> GetAvatarFileAsync(User user);

    Task SaveFileAsync(IFormFile file, string folder, string? fileName = null);

    Task DeleteFileAsync(string fileName, string folder);
}
