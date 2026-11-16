using Taskist.Core.Dtos.Git;

namespace Taskist.Service.Git;

public interface IGitIssueApi
{
    Task<GitIssueDto?> GetIssueAsync(string repoRef, int number);

    Task<List<GitIssueDto>?> ListIssuesAsync(string repoRef, DateTime? updatedSince, string? label, int page);

    Task<GitResult<GitIssueDto>> CreateIssueAsync(string repoRef, GitIssueWriteDto issue);

    Task<GitResult<GitIssueDto>> UpdateIssueAsync(string repoRef, int number, GitIssueWriteDto issue);

    Task<List<GitIssueCommentDto>?> ListIssueCommentsAsync(string repoRef, int number);

    Task<GitResult<GitIssueCommentDto>> CreateIssueCommentAsync(string repoRef, int number, string body);

    Task<GitResult<GitIssueCommentDto>> UpdateIssueCommentAsync(string repoRef, int number, string commentId, string body);
}
