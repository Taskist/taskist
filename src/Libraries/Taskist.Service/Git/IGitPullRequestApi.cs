using Taskist.Core.Dtos.Git;

namespace Taskist.Service.Git;

public interface IGitPullRequestApi
{
    Task<MergeRequestDetailsDto?> GetMergeRequestAsync(string repoRef, int iid);

    Task<List<MergeRequestDetailsDto>?> GetMergeRequestsBySourceBranchAsync(string repoRef, string branchName);

    Task<List<MergeRequestDetailsDto>?> SearchMergeRequestsAsync(string repoRef, string text);

    Task<List<MergeRequestDetailsDto>?> GetOpenMergeRequestsByAuthorAsync(string repoRef, string userName);

    Task<GitResult<MergeRequestDetailsDto>> CreateMergeRequestAsync(string repoRef, string sourceBranch, string targetBranch, string title,
        string? description, string? reviewerUserName, bool removeSourceBranch, bool draft, string userToken);
}
