using Taskist.Core.Dtos.Git;

namespace Taskist.Service.Git;

public interface IGitRepositoryApi
{
    Task<GitRepositoryDto?> GetRepositoryAsync(string repoRef, string? userToken = null);

    Task<List<GitRepositoryDto>> SearchRepositoriesAsync(string? query, string? userToken = null);

    Task<List<BranchDto>?> SearchBranchesAsync(string repoRef, string? search);

    Task<List<BranchDto>?> GetBranchesAsync(string repoRef, int page);

    Task<GitResult<BranchDto>> CreateBranchAsync(string repoRef, string branchName, string baseBranch, string userToken);

    Task<GitResult<object>> DeleteBranchAsync(string repoRef, string branchName, string userToken);

    string? BranchUrl(string? repositoryWebUrl, string branchName);

    bool IsValidRepoRef(string? repoRef);
}
