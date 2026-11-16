using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Taskist.Core.Dtos.Git;
using Taskist.Core.Enums;

namespace Taskist.Service.Git.Providers.GitLab;

public class GitLabProvider : GitProviderBase, IGitProvider
{
    #region Fields

    private static readonly Regex RepoRefPattern = new(@"^(\d+|[A-Za-z0-9_.\-]+(/[A-Za-z0-9_.\-]+)+)$", RegexOptions.Compiled);

    private static readonly GitProviderCapabilities GitLabCapabilities = new()
    {
        Name = "GitLab",
        Term = "merge request",
        ShortTerm = "MR",
        RefPrefix = "!",
        CanRebase = true,
        RebaseLabel = "Rebase",
        CanUnapprove = true,
        RequiredUserScopes = "api",
        RequiredServiceScopes = "read_api (add api for issue sync and reviewer assignment)"
    };

    #endregion

    #region Ctor

    public GitLabProvider(GitProviderContext context) : base(context)
    {
    }

    #endregion

    #region Properties

    public GitProviderType ProviderType => GitProviderType.GitLab;

    public GitProviderCapabilities Capabilities => GitLabCapabilities;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Context.ServiceToken);

    public bool OAuthConfigured => !string.IsNullOrWhiteSpace(Context.Connection.OAuthClientId) && !string.IsNullOrWhiteSpace(Context.OAuthClientSecret);

    protected override string ProviderName => "GitLab";

    #endregion

    #region Repository

    public bool IsValidRepoRef(string? repoRef) => !string.IsNullOrWhiteSpace(repoRef) && RepoRefPattern.IsMatch(repoRef.Trim());

    public async Task<GitRepositoryDto?> GetRepositoryAsync(string repoRef, string? userToken = null)
    {
        if (!IsValidRepoRef(repoRef))
            return Invalid<GitRepositoryDto>();

        return await GetAsync<GitRepositoryDto>($"projects/{Escape(repoRef)}", userToken);
    }

    public async Task<List<GitRepositoryDto>> SearchRepositoriesAsync(string? query, string? userToken = null)
    {
        return await GetAsync<List<GitRepositoryDto>>(
            $"projects?search={Escape(query)}&membership=true&per_page=20&order_by=last_activity_at", userToken) ?? [];
    }

    public async Task<List<BranchDto>?> SearchBranchesAsync(string repoRef, string? search)
    {
        if (!IsValidRepoRef(repoRef))
            return Invalid<List<BranchDto>>();

        var query = string.IsNullOrWhiteSpace(search) ? string.Empty : $"&search={Escape(search)}";
        return await GetAsync<List<BranchDto>>($"projects/{Escape(repoRef)}/repository/branches?per_page=30{query}");
    }

    public async Task<List<BranchDto>?> GetBranchesAsync(string repoRef, int page)
    {
        if (!IsValidRepoRef(repoRef))
            return Invalid<List<BranchDto>>();

        return await GetAsync<List<BranchDto>>($"projects/{Escape(repoRef)}/repository/branches?per_page=100&page={Math.Max(1, page)}");
    }

    public async Task<GitResult<BranchDto>> CreateBranchAsync(string repoRef, string branchName, string baseBranch, string userToken)
    {
        if (!IsValidRepoRef(repoRef))
            return Remember(GitResult<BranchDto>.Fail("The repository reference is not valid."));

        return await SendAsync<BranchDto>(HttpMethod.Post,
            $"projects/{Escape(repoRef)}/repository/branches?branch={Escape(branchName)}&ref={Escape(baseBranch)}", userToken, personal: true);
    }

    public async Task<GitResult<object>> DeleteBranchAsync(string repoRef, string branchName, string userToken)
    {
        if (!IsValidRepoRef(repoRef))
            return Remember(GitResult<object>.Fail("The repository reference is not valid."));

        return await SendAsync<object>(HttpMethod.Delete, $"projects/{Escape(repoRef)}/repository/branches/{Escape(branchName)}", userToken, personal: true);
    }

    public string? BranchUrl(string? repositoryWebUrl, string branchName)
    {
        return string.IsNullOrWhiteSpace(repositoryWebUrl) ? null : $"{repositoryWebUrl.TrimEnd('/')}/-/tree/{Uri.EscapeDataString(branchName)}";
    }

    #endregion

    #region Merge requests

    public async Task<MergeRequestDetailsDto?> GetMergeRequestAsync(string repoRef, int iid)
    {
        if (!IsValidRepoRef(repoRef) || iid <= 0)
            return Invalid<MergeRequestDetailsDto>();

        return await GetAsync<MergeRequestDetailsDto>($"projects/{Escape(repoRef)}/merge_requests/{iid}");
    }

    public async Task<List<MergeRequestDetailsDto>?> GetMergeRequestsBySourceBranchAsync(string repoRef, string branchName)
    {
        if (!IsValidRepoRef(repoRef))
            return Invalid<List<MergeRequestDetailsDto>>();

        return await GetAsync<List<MergeRequestDetailsDto>>(
            $"projects/{Escape(repoRef)}/merge_requests?state=all&source_branch={Escape(branchName)}&per_page=20&order_by=updated_at");
    }

    public async Task<List<MergeRequestDetailsDto>?> SearchMergeRequestsAsync(string repoRef, string text)
    {
        if (!IsValidRepoRef(repoRef))
            return Invalid<List<MergeRequestDetailsDto>>();

        return await GetAsync<List<MergeRequestDetailsDto>>(
            $"projects/{Escape(repoRef)}/merge_requests?state=all&search={Escape(text)}&in=title,description&per_page=20&order_by=updated_at");
    }

    public async Task<List<MergeRequestDetailsDto>?> GetOpenMergeRequestsByAuthorAsync(string repoRef, string userName)
    {
        if (!IsValidRepoRef(repoRef))
            return Invalid<List<MergeRequestDetailsDto>>();

        return await GetAsync<List<MergeRequestDetailsDto>>(
            $"projects/{Escape(repoRef)}/merge_requests?state=opened&author_username={Escape(userName)}&per_page=50");
    }

    public async Task<GitResult<MergeRequestDetailsDto>> CreateMergeRequestAsync(string repoRef, string sourceBranch, string targetBranch, string title,
        string? description, string? reviewerUserName, bool removeSourceBranch, bool draft, string userToken)
    {
        if (!IsValidRepoRef(repoRef))
            return Remember(GitResult<MergeRequestDetailsDto>.Fail("The repository reference is not valid."));

        var finalTitle = draft && !title.StartsWith("Draft:", StringComparison.OrdinalIgnoreCase) ? "Draft: " + title : title;
        var body = new Dictionary<string, object>
        {
            ["source_branch"] = sourceBranch,
            ["target_branch"] = targetBranch,
            ["title"] = finalTitle,
            ["remove_source_branch"] = removeSourceBranch
        };
        if (!string.IsNullOrWhiteSpace(description))
            body["description"] = description;

        if (!string.IsNullOrWhiteSpace(reviewerUserName))
        {
            var reviewer = await FindUserAsync(reviewerUserName);
            if (reviewer != null)
                body["reviewer_ids"] = new[] { reviewer.Id };
        }

        return await SendAsync<MergeRequestDetailsDto>(HttpMethod.Post, $"projects/{Escape(repoRef)}/merge_requests", userToken, body, personal: true);
    }

    #endregion

    #region Review

    public async Task<List<ReviewMergeRequestDto>?> GetOpenReviewMergeRequestsAsync(string repoRef)
    {
        if (!IsValidRepoRef(repoRef))
            return Invalid<List<ReviewMergeRequestDto>>();

        return await GetAsync<List<ReviewMergeRequestDto>>($"projects/{Escape(repoRef)}/merge_requests?state=opened&per_page=100&order_by=updated_at");
    }

    public async Task<List<ReviewMergeRequestDto>?> GetUserMergeRequestsAsync(GitUserMergeRequestQuery query, string userToken)
    {
        var path = query.ReviewRequested
            ? $"merge_requests?scope=all&state=opened&reviewer_username={Escape(query.UserName)}&order_by=updated_at&per_page={query.Limit}"
            : $"merge_requests?scope=created_by_me&state={(query.OpenOnly ? "opened" : "all")}&order_by=updated_at&per_page={query.Limit}";

        var result = await SendAsync<List<ReviewMergeRequestDto>>(HttpMethod.Get, path, userToken, personal: true);
        return result.Success ? result.Data : null;
    }

    public async Task<ReviewMergeRequestDto?> GetReviewMergeRequestAsync(string repoRef, int iid, string? userToken)
    {
        if (!IsValidRepoRef(repoRef))
            return Invalid<ReviewMergeRequestDto>();

        return await GetAsync<ReviewMergeRequestDto>(
            $"projects/{Escape(repoRef)}/merge_requests/{iid}?include_diverged_commits_count=true&include_rebase_in_progress=true&with_merge_status_recheck=true", userToken);
    }

    public async Task<MergeRequestChangesDto?> GetChangesAsync(string repoRef, int iid, string? userToken)
    {
        if (!IsValidRepoRef(repoRef))
            return Invalid<MergeRequestChangesDto>();

        return await GetAsync<MergeRequestChangesDto>($"projects/{Escape(repoRef)}/merge_requests/{iid}/changes", userToken);
    }

    public async Task<List<MergeRequestCommitDto>?> GetCommitsAsync(string repoRef, int iid, string? userToken)
    {
        if (!IsValidRepoRef(repoRef))
            return Invalid<List<MergeRequestCommitDto>>();

        return await GetAsync<List<MergeRequestCommitDto>>($"projects/{Escape(repoRef)}/merge_requests/{iid}/commits?per_page=100", userToken);
    }

    public async Task<List<MergeRequestNoteDto>?> GetNotesAsync(string repoRef, int iid, string? userToken)
    {
        if (!IsValidRepoRef(repoRef))
            return Invalid<List<MergeRequestNoteDto>>();

        return await GetAsync<List<MergeRequestNoteDto>>(
            $"projects/{Escape(repoRef)}/merge_requests/{iid}/notes?sort=asc&order_by=created_at&per_page=100", userToken);
    }

    public async Task<MergeRequestApprovalsDto?> GetApprovalsAsync(string repoRef, int iid, string? userToken)
    {
        if (!IsValidRepoRef(repoRef))
            return Invalid<MergeRequestApprovalsDto>();

        return await GetAsync<MergeRequestApprovalsDto>($"projects/{Escape(repoRef)}/merge_requests/{iid}/approvals", userToken);
    }

    public Task<GitResult<object>> ApproveAsync(string repoRef, int iid, string userToken) =>
        UserActionAsync(HttpMethod.Post, repoRef, $"merge_requests/{iid}/approve", userToken);

    public Task<GitResult<object>> UnapproveAsync(string repoRef, int iid, string userToken) =>
        UserActionAsync(HttpMethod.Post, repoRef, $"merge_requests/{iid}/unapprove", userToken);

    public Task<GitResult<object>> AddNoteAsync(string repoRef, int iid, string body, string userToken) =>
        UserActionAsync(HttpMethod.Post, repoRef, $"merge_requests/{iid}/notes", userToken, new { body });

    public async Task<GitResult<object>> RequestChangesAsync(string repoRef, int iid, string body, string userToken)
    {
        var note = await AddNoteAsync(repoRef, iid, body, userToken);
        if (!note.Success)
            return note;

        var approvals = await GetApprovalsAsync(repoRef, iid, userToken);
        if (approvals?.UserHasApproved == true)
            await UnapproveAsync(repoRef, iid, userToken);

        return note;
    }

    public Task<GitResult<object>> MergeAsync(string repoRef, int iid, string? sha, bool squash, bool removeSourceBranch, string userToken)
    {
        var body = new Dictionary<string, object>
        {
            ["squash"] = squash,
            ["should_remove_source_branch"] = removeSourceBranch
        };
        if (!string.IsNullOrWhiteSpace(sha))
            body["sha"] = sha;

        return UserActionAsync(HttpMethod.Put, repoRef, $"merge_requests/{iid}/merge", userToken, body);
    }

    public Task<GitResult<object>> RebaseAsync(string repoRef, int iid, string userToken) =>
        UserActionAsync(HttpMethod.Put, repoRef, $"merge_requests/{iid}/rebase", userToken);

    public Task<GitResult<object>> CloseAsync(string repoRef, int iid, string userToken) =>
        UserActionAsync(HttpMethod.Put, repoRef, $"merge_requests/{iid}", userToken, new { state_event = "close" });

    public async Task<GitResult<object>> SetReviewerAsync(string repoRef, int iid, GitPersonDto reviewer)
    {
        if (!IsValidRepoRef(repoRef))
            return Remember(GitResult<object>.Fail("The repository reference is not valid."));

        return await SendAsync<object>(HttpMethod.Put, $"projects/{Escape(repoRef)}/merge_requests/{iid}",
            await ServiceTokenAsync(null), new { reviewer_ids = new[] { reviewer.Id } });
    }

    #endregion

    #region Users

    public async Task<GitPersonDto?> FindUserAsync(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return null;

        return (await GetAsync<List<GitPersonDto>>($"users?username={Escape(userName)}"))?.FirstOrDefault();
    }

    public async Task<GitPersonDto?> GetUserByIdAsync(long id)
    {
        return id <= 0 ? null : await GetAsync<GitPersonDto>($"users/{id}");
    }

    public async Task<List<GitPersonDto>> SearchUsersAsync(string? query)
    {
        return await GetAsync<List<GitPersonDto>>($"users?active=true&search={Escape(query)}&per_page=20") ?? [];
    }

    public async Task<GitResult<GitTokenInfoDto>> ValidateUserTokenAsync(string userToken)
    {
        var user = await SendAsync<GitPersonDto>(HttpMethod.Get, "user", userToken, personal: true);
        if (!user.Success || user.Data == null)
            return GitResult<GitTokenInfoDto>.Fail(user.Error ?? "GitLab did not accept this token.", user.StatusCode);

        var info = new GitTokenInfoDto
        {
            ExternalUserId = user.Data.Id.ToString(),
            UserName = user.Data.UserName ?? string.Empty,
            DisplayName = user.Data.Name
        };

        var self = await SendRawAsync(HttpMethod.Get, "personal_access_tokens/self", userToken);
        if (self.Success)
        {
            var token = Deserialize<JObject>(self.Content);
            info.Scopes = token?["scopes"]?.Values<string>().Where(s => s != null).Select(s => s!).ToList() ?? [];
            info.ExpiresAt = token?["expires_at"]?.Type == JTokenType.Null ? null : token?["expires_at"]?.ToObject<DateTime?>();
            if (info.Scopes.Count > 0 && !info.Scopes.Contains("api"))
                info.Problem = $"This token only has the scope(s) {string.Join(", ", info.Scopes)}. Taskist needs the 'api' scope to create branches and merge requests. Create a new token and tick 'api'.";
        }

        return GitResult<GitTokenInfoDto>.Ok(info);
    }

    public async Task<GitResult<GitPersonDto>> GetServiceIdentityAsync()
    {
        return await SendAsync<GitPersonDto>(HttpMethod.Get, "user", await ServiceTokenAsync(null));
    }

    #endregion

    #region Issues

    public async Task<GitIssueDto?> GetIssueAsync(string repoRef, int number)
    {
        if (!IsValidRepoRef(repoRef))
            return Invalid<GitIssueDto>();

        var issue = await GetAsync<JObject>($"projects/{Escape(repoRef)}/issues/{number}");
        return issue == null ? null : MapIssue(issue);
    }

    public async Task<List<GitIssueDto>?> ListIssuesAsync(string repoRef, DateTime? updatedSince, string? label, int page)
    {
        if (!IsValidRepoRef(repoRef))
            return Invalid<List<GitIssueDto>>();

        var path = $"projects/{Escape(repoRef)}/issues?scope=all&order_by=updated_at&sort=asc&per_page=100&page={Math.Max(1, page)}";
        if (updatedSince != null)
            path += $"&updated_after={Escape(updatedSince.Value.ToUniversalTime().ToString("o"))}";
        if (!string.IsNullOrWhiteSpace(label))
            path += $"&labels={Escape(label)}";

        var list = await GetAsync<JArray>(path);
        return list?.OfType<JObject>().Select(MapIssue).ToList();
    }

    public async Task<GitResult<GitIssueDto>> CreateIssueAsync(string repoRef, GitIssueWriteDto issue)
    {
        if (!IsValidRepoRef(repoRef))
            return Remember(GitResult<GitIssueDto>.Fail("The repository reference is not valid."));

        var result = await SendAsync<JObject>(HttpMethod.Post, $"projects/{Escape(repoRef)}/issues", await ServiceTokenAsync(null), IssueBody(issue, false));
        return result.Success && result.Data != null
            ? GitResult<GitIssueDto>.Ok(MapIssue(result.Data))
            : GitResult<GitIssueDto>.Fail(result.Error ?? "GitLab did not create the issue.", result.StatusCode);
    }

    public async Task<GitResult<GitIssueDto>> UpdateIssueAsync(string repoRef, int number, GitIssueWriteDto issue)
    {
        if (!IsValidRepoRef(repoRef))
            return Remember(GitResult<GitIssueDto>.Fail("The repository reference is not valid."));

        var result = await SendAsync<JObject>(HttpMethod.Put, $"projects/{Escape(repoRef)}/issues/{number}", await ServiceTokenAsync(null), IssueBody(issue, true));
        return result.Success && result.Data != null
            ? GitResult<GitIssueDto>.Ok(MapIssue(result.Data))
            : GitResult<GitIssueDto>.Fail(result.Error ?? "GitLab did not update the issue.", result.StatusCode);
    }

    public async Task<List<GitIssueCommentDto>?> ListIssueCommentsAsync(string repoRef, int number)
    {
        if (!IsValidRepoRef(repoRef))
            return Invalid<List<GitIssueCommentDto>>();

        var list = await GetAsync<JArray>($"projects/{Escape(repoRef)}/issues/{number}/notes?sort=asc&order_by=created_at&per_page=100");
        return list?.OfType<JObject>().Select(MapComment).ToList();
    }

    public async Task<GitResult<GitIssueCommentDto>> CreateIssueCommentAsync(string repoRef, int number, string body)
    {
        if (!IsValidRepoRef(repoRef))
            return Remember(GitResult<GitIssueCommentDto>.Fail("The repository reference is not valid."));

        var result = await SendAsync<JObject>(HttpMethod.Post, $"projects/{Escape(repoRef)}/issues/{number}/notes", await ServiceTokenAsync(null), new { body });
        return result.Success && result.Data != null
            ? GitResult<GitIssueCommentDto>.Ok(MapComment(result.Data))
            : GitResult<GitIssueCommentDto>.Fail(result.Error ?? "GitLab did not add the comment.", result.StatusCode);
    }

    public async Task<GitResult<GitIssueCommentDto>> UpdateIssueCommentAsync(string repoRef, int number, string commentId, string body)
    {
        if (!IsValidRepoRef(repoRef) || !long.TryParse(commentId, out var id))
            return Remember(GitResult<GitIssueCommentDto>.Fail("The comment reference is not valid."));

        var result = await SendAsync<JObject>(HttpMethod.Put, $"projects/{Escape(repoRef)}/issues/{number}/notes/{id}", await ServiceTokenAsync(null), new { body });
        return result.Success && result.Data != null
            ? GitResult<GitIssueCommentDto>.Ok(MapComment(result.Data))
            : GitResult<GitIssueCommentDto>.Fail(result.Error ?? "GitLab did not update the comment.", result.StatusCode);
    }

    #endregion

    #region OAuth

    public string BuildAuthorizeUrl(string redirectUri, string state, string codeChallenge)
    {
        return $"{Context.WebBase}/oauth/authorize?client_id={Escape(Context.Connection.OAuthClientId)}&redirect_uri={Escape(redirectUri)}" +
               $"&response_type=code&scope=api&state={Escape(state)}&code_challenge={Escape(codeChallenge)}&code_challenge_method=S256";
    }

    public async Task<GitResult<GitOAuthTokenDto>> ExchangeCodeAsync(string code, string redirectUri, string codeVerifier)
    {
        return await TokenRequestAsync(new Dictionary<string, string>
        {
            ["client_id"] = Context.Connection.OAuthClientId ?? string.Empty,
            ["client_secret"] = Context.OAuthClientSecret ?? string.Empty,
            ["code"] = code,
            ["grant_type"] = "authorization_code",
            ["redirect_uri"] = redirectUri,
            ["code_verifier"] = codeVerifier
        });
    }

    public async Task<GitResult<GitOAuthTokenDto>> RefreshTokenAsync(string refreshToken)
    {
        return await TokenRequestAsync(new Dictionary<string, string>
        {
            ["client_id"] = Context.Connection.OAuthClientId ?? string.Empty,
            ["client_secret"] = Context.OAuthClientSecret ?? string.Empty,
            ["refresh_token"] = refreshToken,
            ["grant_type"] = "refresh_token"
        });
    }

    public async Task RevokeTokenAsync(string token, bool oauth)
    {
        if (oauth && OAuthConfigured)
        {
            await SendRawAsync(HttpMethod.Post, $"{Context.WebBase}/oauth/revoke", null, new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = Context.Connection.OAuthClientId!,
                ["client_secret"] = Context.OAuthClientSecret!,
                ["token"] = token
            }), absolute: true);
            return;
        }

        if (!oauth)
            await SendRawAsync(HttpMethod.Delete, "personal_access_tokens/self", token);
    }

    #endregion

    #region Webhooks

    public async Task<GitResult<object>> RegisterWebhookAsync(string repoRef, GitWebhookRegistrationDto registration, string userToken)
    {
        if (!IsValidRepoRef(repoRef))
            return Remember(GitResult<object>.Fail("The repository reference is not valid."));

        return await SendAsync<object>(HttpMethod.Post, $"projects/{Escape(repoRef)}/hooks", userToken, new
        {
            url = registration.Url,
            token = registration.Secret,
            push_events = true,
            merge_requests_events = true,
            note_events = true,
            issues_events = registration.IssueEvents,
            enable_ssl_verification = true
        }, personal: true);
    }

    #endregion

    #region Utilities

    protected override Task<string?> ServiceTokenAsync(string? path) => Task.FromResult(Context.ServiceToken);

    private T? Invalid<T>() where T : class
    {
        LastError = "The repository reference is not valid.";
        return null;
    }

    private async Task<GitResult<object>> UserActionAsync(HttpMethod method, string repoRef, string path, string userToken, object? body = null)
    {
        if (!IsValidRepoRef(repoRef))
            return Remember(GitResult<object>.Fail("The repository reference is not valid."));

        return await SendAsync<object>(method, $"projects/{Escape(repoRef)}/{path}", userToken, body, personal: true);
    }

    private async Task<GitResult<GitOAuthTokenDto>> TokenRequestAsync(Dictionary<string, string> form)
    {
        if (!OAuthConfigured)
            return GitResult<GitOAuthTokenDto>.Fail("OAuth is not configured for this connection.");

        var response = await SendRawAsync(HttpMethod.Post, $"{Context.WebBase}/oauth/token", null, new FormUrlEncodedContent(form), absolute: true);
        if (!response.Success)
            return GitResult<GitOAuthTokenDto>.Fail(DescribeError(response.StatusCode, response.Content, true), response.StatusCode);

        var json = Deserialize<JObject>(response.Content);
        var accessToken = json?["access_token"]?.ToString();
        if (string.IsNullOrWhiteSpace(accessToken))
            return GitResult<GitOAuthTokenDto>.Fail("GitLab did not return an access token.");

        var expiresIn = json?["expires_in"]?.Value<long?>();
        return GitResult<GitOAuthTokenDto>.Ok(new GitOAuthTokenDto
        {
            AccessToken = accessToken,
            RefreshToken = json?["refresh_token"]?.ToString(),
            ExpiresOn = expiresIn > 0 ? DateTime.UtcNow.AddSeconds(expiresIn.Value) : null,
            Scopes = json?["scope"]?.ToString()
        });
    }

    private static object IssueBody(GitIssueWriteDto issue, bool update)
    {
        var body = new Dictionary<string, object?>();
        if (issue.Title != null)
            body["title"] = issue.Title;
        if (issue.Body != null)
            body["description"] = issue.Body;
        if (issue.Labels != null)
            body["labels"] = string.Join(",", issue.Labels);
        if (issue.Assignees != null)
            body["assignee_ids"] = issue.Assignees.Where(a => a.Id > 0).Select(a => a.Id).ToArray();
        if (update && issue.Closed != null)
            body["state_event"] = issue.Closed.Value ? "close" : "reopen";
        return body;
    }

    public static GitIssueDto MapIssue(JObject issue)
    {
        return new GitIssueDto
        {
            ExternalId = issue["id"]?.ToString() ?? string.Empty,
            Number = issue["iid"]?.Value<int>() ?? 0,
            Title = issue["title"]?.ToString() ?? string.Empty,
            Body = issue["description"]?.Type == JTokenType.Null ? null : issue["description"]?.ToString(),
            Closed = string.Equals(issue["state"]?.ToString(), "closed", StringComparison.OrdinalIgnoreCase),
            Labels = Labels(issue["labels"]),
            Assignees = (issue["assignees"] as JArray)?.OfType<JObject>().Select(Person).ToList() ?? [],
            Author = issue["author"] is JObject author ? Person(author) : null,
            Url = issue["web_url"]?.ToString() ?? issue["url"]?.ToString(),
            CreatedAt = issue["created_at"]?.ToObject<DateTime?>() ?? DateTime.UtcNow,
            UpdatedAt = issue["updated_at"]?.ToObject<DateTime?>() ?? DateTime.UtcNow
        };
    }

    public static GitIssueCommentDto MapComment(JObject note)
    {
        return new GitIssueCommentDto
        {
            ExternalId = note["id"]?.ToString() ?? string.Empty,
            Body = note["body"]?.ToString() ?? note["note"]?.ToString(),
            Author = note["author"] is JObject author ? Person(author) : null,
            System = note["system"]?.Type == JTokenType.Boolean && note["system"]!.Value<bool>(),
            Url = note["url"]?.ToString(),
            CreatedAt = note["created_at"]?.ToObject<DateTime?>() ?? DateTime.UtcNow,
            UpdatedAt = note["updated_at"]?.ToObject<DateTime?>() ?? DateTime.UtcNow
        };
    }

    public static GitPersonDto Person(JObject user)
    {
        return new GitPersonDto
        {
            Id = user["id"]?.Value<long?>() ?? 0,
            UserName = user["username"]?.ToString(),
            Name = user["name"]?.ToString(),
            AvatarUrl = user["avatar_url"]?.ToString(),
            WebUrl = user["web_url"]?.ToString(),
            IsBot = user["bot"]?.Type == JTokenType.Boolean && user["bot"]!.Value<bool>()
        };
    }

    private static List<string> Labels(JToken? labels)
    {
        if (labels is not JArray array)
            return [];

        return array
            .Select(l => l.Type == JTokenType.String ? l.ToString() : l["title"]?.ToString() ?? l["name"]?.ToString())
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(l => l!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    #endregion
}
