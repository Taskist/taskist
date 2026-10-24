using Taskist.Core.Dtos.Git;

namespace Taskist.Service.Git;

public interface IGitUserApi
{
    Task<GitPersonDto?> FindUserAsync(string userName);

    Task<GitPersonDto?> GetUserByIdAsync(long id);

    Task<List<GitPersonDto>> SearchUsersAsync(string? query);

    Task<GitResult<GitTokenInfoDto>> ValidateUserTokenAsync(string userToken);

    Task<GitResult<GitPersonDto>> GetServiceIdentityAsync();
}
