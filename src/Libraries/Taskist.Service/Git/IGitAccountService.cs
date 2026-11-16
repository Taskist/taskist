using Taskist.Core.Domain.Git;
using Taskist.Core.Domain.Users;
using Taskist.Core.Dtos.Git;

namespace Taskist.Service.Git;

public interface IGitAccountService
{
    Task<IList<GitAccountStatusDto>> GetStatusesAsync(User user);

    Task<GitAccountStatusDto> GetStatusAsync(User user, GitConnection connection);

    Task<string?> GetTokenAsync(User? user, GitConnection connection);

    Task<(string? Token, string? Error)> RequireTokenAsync(User? user, GitConnection connection, string action);

    Task<(bool Success, string Message)> ConnectWithTokenAsync(User user, int connectionId, string token);

    Task<(bool Success, string Message)> ConnectWithOAuthAsync(User user, int connectionId, GitOAuthTokenDto token);

    Task DisconnectAsync(User user, int connectionId);

    Task<string?> GetUserNameAsync(User? user, GitConnection connection);

    Task<User?> FindUserAsync(GitConnection connection, string? userName);

    Task<IList<int>> GetConnectedConnectionIdsAsync(int userId);
}
