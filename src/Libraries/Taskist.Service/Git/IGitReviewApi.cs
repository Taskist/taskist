using Taskist.Core.Dtos.Git;

namespace Taskist.Service.Git;

public interface IGitReviewApi
{
    Task<List<ReviewMergeRequestDto>?> GetOpenReviewMergeRequestsAsync(string repoRef);

    Task<List<ReviewMergeRequestDto>?> GetUserMergeRequestsAsync(GitUserMergeRequestQuery query, string userToken);

    Task<ReviewMergeRequestDto?> GetReviewMergeRequestAsync(string repoRef, int iid, string? userToken);

    Task<MergeRequestChangesDto?> GetChangesAsync(string repoRef, int iid, string? userToken);

    Task<List<MergeRequestCommitDto>?> GetCommitsAsync(string repoRef, int iid, string? userToken);

    Task<List<MergeRequestNoteDto>?> GetNotesAsync(string repoRef, int iid, string? userToken);

    Task<MergeRequestApprovalsDto?> GetApprovalsAsync(string repoRef, int iid, string? userToken);

    Task<GitResult<object>> ApproveAsync(string repoRef, int iid, string userToken);

    Task<GitResult<object>> UnapproveAsync(string repoRef, int iid, string userToken);

    Task<GitResult<object>> AddNoteAsync(string repoRef, int iid, string body, string userToken);

    Task<GitResult<object>> RequestChangesAsync(string repoRef, int iid, string body, string userToken);

    Task<GitResult<object>> MergeAsync(string repoRef, int iid, string? sha, bool squash, bool removeSourceBranch, string userToken);

    Task<GitResult<object>> RebaseAsync(string repoRef, int iid, string userToken);

    Task<GitResult<object>> CloseAsync(string repoRef, int iid, string userToken);

    Task<GitResult<object>> SetReviewerAsync(string repoRef, int iid, GitPersonDto reviewer);
}
