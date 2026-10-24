using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Taskist.Core.Dtos.Git;
using Taskist.Core.Enums;

namespace Taskist.Service.Git.Providers.GitHub;

public class GitHubProvider : GitProviderBase, IGitProvider
{
    #region Fields

    private const int DetailLimit = 30;

    private static readonly Regex RepoRefPattern = new(@"^[A-Za-z0-9](?:[A-Za-z0-9\-]{0,38})/[A-Za-z0-9_.\-]{1,100}$", RegexOptions.Compiled);

    private static readonly GitProviderCapabilities GitHubCapabilities = new()
    {
        Name = "GitHub",
        Term = "pull request",
        ShortTerm = "PR",
        RefPrefix = "#",
        CanRebase = true,
        RebaseLabel = "Update branch",
        CanUnapprove = false,
        RequiredUserScopes = "repo (classic) or Contents + Pull requests: read & write (fine-grained)",
        RequiredServiceScopes = "GitHub App: Contents read, Pull requests read & write, Issues read & write, Metadata read"
    };

    private readonly Dictionary<string, string?> _defaultBranches = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string?> _tokenLogins = new();

    #endregion

    #region Ctor

    public GitHubProvider(GitProviderContext context) : base(context)
    {
    }

    #endregion

    #region Properties

    public GitProviderType ProviderType => GitProviderType.GitHub;

    public GitProviderCapabilities Capabilities => GitHubCapabilities;

    public bool IsConfigured => Context.Connection.AuthMode == (int)GitAuthMode.GitHubApp
        ? !string.IsNullOrWhiteSpace(Context.Connection.AppId) && !string.IsNullOrWhiteSpace(Context.AppPrivateKey)
        : !string.IsNullOrWhiteSpace(Context.ServiceToken);

    public bool OAuthConfigured => !string.IsNullOrWhiteSpace(Context.Connection.OAuthClientId) && !string.IsNullOrWhiteSpace(Context.OAuthClientSecret);

    protected override string ProviderName => "GitHub";

    #endregion

    #region Repository

    public bool IsValidRepoRef(string? repoRef) => !string.IsNullOrWhiteSpace(repoRef) && RepoRefPattern.IsMatch(repoRef.Trim());

    public async Task<GitRepositoryDto?> GetRepositoryAsync(string repoRef, string? userToken = null)
    {
        if (!IsValidRepoRef(repoRef))
            return Invalid<GitRepositoryDto>();

        var repo = await GetAsync<JObject>($"repos/{Repo(repoRef)}", userToken);
        return repo == null ? null : MapRepository(repo);
    }

    public async Task<List<GitRepositoryDto>> SearchRepositoriesAsync(string? query, string? userToken = null)
    {
        JArray? list;
        if (userToken == null && Context.Connection.AuthMode == (int)GitAuthMode.GitHubApp)
            list = (await GetAsync<JObject>("installation/repositories?per_page=100"))?["repositories"] as JArray;
        else
            list = await GetAsync<JArray>("user/repos?per_page=100&sort=updated&affiliation=owner,collaborator,organization_member", userToken);

        var term = query?.Trim() ?? string.Empty;
        return (list ?? [])
            .OfType<JObject>()
            .Select(MapRepository)
            .Where(r => term.Length == 0 || r.PathWithNamespace.Contains(term, StringComparison.OrdinalIgnoreCase))
            .Take(20)
            .ToList();
    }

    public async Task<List<BranchDto>?> SearchBranchesAsync(string repoRef, string? search)
    {
        if (!IsValidRepoRef(repoRef))
            return Invalid<List<BranchDto>>();

        var all = new List<BranchDto>();
        for (var page = 1; page <= 3; page++)
        {
            var list = await GetBranchesAsync(repoRef, page);
            if (list == null)
                return page == 1 ? null : all;

            all.AddRange(list);
            if (list.Count < 100)
                break;
        }

        var term = search?.Trim() ?? string.Empty;
        return all.Where(b => term.Length == 0 || b.Name.Contains(term, StringComparison.OrdinalIgnoreCase)).Take(30).ToList();
    }

    public async Task<List<BranchDto>?> GetBranchesAsync(string repoRef, int page)
    {
        if (!IsValidRepoRef(repoRef))
            return Invalid<List<BranchDto>>();

        var list = await GetAsync<JArray>($"repos/{Repo(repoRef)}/branches?per_page=100&page={Math.Max(1, page)}");
        if (list == null)
            return null;

        var defaultBranch = await DefaultBranchAsync(repoRef);
        var webUrl = $"{Context.WebBase}/{repoRef.Trim()}";
        return list.OfType<JObject>().Select(b =>
        {
            var name = b["name"]?.ToString() ?? string.Empty;
            return new BranchDto
            {
                Name = name,
                Default = string.Equals(name, defaultBranch, StringComparison.Ordinal),
                Protected = b["protected"]?.Value<bool?>() ?? false,
                Url = BranchUrl(webUrl, name)
            };
        }).ToList();
    }

    public async Task<GitResult<BranchDto>> CreateBranchAsync(string repoRef, string branchName, string baseBranch, string userToken)
    {
        if (!IsValidRepoRef(repoRef))
            return Remember(GitResult<BranchDto>.Fail("The repository reference is not valid."));

        var reference = await SendAsync<JObject>(HttpMethod.Get, $"repos/{Repo(repoRef)}/git/ref/heads/{EscapePath(baseBranch)}", userToken, personal: true);
        var sha = reference.Data?["object"]?["sha"]?.ToString();
        if (!reference.Success || string.IsNullOrWhiteSpace(sha))
            return GitResult<BranchDto>.Fail(reference.Error ?? $"Branch '{baseBranch}' was not found.", reference.StatusCode);

        var created = await SendAsync<JObject>(HttpMethod.Post, $"repos/{Repo(repoRef)}/git/refs", userToken,
            new { @ref = $"refs/heads/{branchName}", sha }, personal: true);
        if (!created.Success)
            return GitResult<BranchDto>.Fail(created.Error ?? "GitHub did not create the branch.", created.StatusCode);

        return GitResult<BranchDto>.Ok(new BranchDto { Name = branchName, Url = BranchUrl($"{Context.WebBase}/{repoRef.Trim()}", branchName) }, created.StatusCode);
    }

    public async Task<GitResult<object>> DeleteBranchAsync(string repoRef, string branchName, string userToken)
    {
        if (!IsValidRepoRef(repoRef))
            return Remember(GitResult<object>.Fail("The repository reference is not valid."));

        return await SendAsync<object>(HttpMethod.Delete, $"repos/{Repo(repoRef)}/git/refs/heads/{EscapePath(branchName)}", userToken, personal: true);
    }

    public string? BranchUrl(string? repositoryWebUrl, string branchName)
    {
        return string.IsNullOrWhiteSpace(repositoryWebUrl) ? null : $"{repositoryWebUrl.TrimEnd('/')}/tree/{EscapePath(branchName)}";
    }

    #endregion

    #region Pull requests

    public async Task<MergeRequestDetailsDto?> GetMergeRequestAsync(string repoRef, int iid)
    {
        if (!IsValidRepoRef(repoRef) || iid <= 0)
            return Invalid<MergeRequestDetailsDto>();

        var pull = await GetAsync<JObject>($"repos/{Repo(repoRef)}/pulls/{iid}");
        return pull == null ? null : MapDetails(pull);
    }

    public async Task<List<MergeRequestDetailsDto>?> GetMergeRequestsBySourceBranchAsync(string repoRef, string branchName)
    {
        if (!IsValidRepoRef(repoRef))
            return Invalid<List<MergeRequestDetailsDto>>();

        var owner = repoRef.Trim().Split('/')[0];
        var list = await GetAsync<JArray>($"repos/{Repo(repoRef)}/pulls?state=all&head={Escape($"{owner}:{branchName}")}&per_page=20&sort=updated&direction=desc");
        return list?.OfType<JObject>().Select(MapDetails).ToList();
    }

    public async Task<List<MergeRequestDetailsDto>?> SearchMergeRequestsAsync(string repoRef, string text)
    {
        if (!IsValidRepoRef(repoRef))
            return Invalid<List<MergeRequestDetailsDto>>();

        var query = $"repo:{repoRef.Trim()} is:pr {text} in:title,body";
        var result = await GetAsync<JObject>($"search/issues?q={Escape(query)}&per_page=10&sort=updated&order=desc");
        if (result == null)
            return null;

        var items = new List<MergeRequestDetailsDto>();
        foreach (var item in (result["items"] as JArray ?? []).OfType<JObject>().Take(10))
        {
            var number = item["number"]?.Value<int?>() ?? 0;
            var detail = number > 0 ? await GetMergeRequestAsync(repoRef, number) : null;
            if (detail != null)
                items.Add(detail);
        }

        return items;
    }

    public async Task<List<MergeRequestDetailsDto>?> GetOpenMergeRequestsByAuthorAsync(string repoRef, string userName)
    {
        if (!IsValidRepoRef(repoRef))
            return Invalid<List<MergeRequestDetailsDto>>();

        var list = await GetAsync<JArray>($"repos/{Repo(repoRef)}/pulls?state=open&per_page=100&sort=updated&direction=desc");
        return list?.OfType<JObject>()
            .Where(p => string.Equals(p["user"]?["login"]?.ToString(), userName, StringComparison.OrdinalIgnoreCase))
            .Select(MapDetails)
            .ToList();
    }

    public async Task<GitResult<MergeRequestDetailsDto>> CreateMergeRequestAsync(string repoRef, string sourceBranch, string targetBranch, string title,
        string? description, string? reviewerUserName, bool removeSourceBranch, bool draft, string userToken)
    {
        if (!IsValidRepoRef(repoRef))
            return Remember(GitResult<MergeRequestDetailsDto>.Fail("The repository reference is not valid."));

        var created = await SendAsync<JObject>(HttpMethod.Post, $"repos/{Repo(repoRef)}/pulls", userToken, new
        {
            title,
            head = sourceBranch,
            @base = targetBranch,
            body = description ?? string.Empty,
            draft
        }, personal: true);

        if (!created.Success || created.Data == null)
            return GitResult<MergeRequestDetailsDto>.Fail(created.Error ?? "GitHub did not create the pull request.", created.StatusCode);

        var details = MapDetails(created.Data);
        if (!string.IsNullOrWhiteSpace(reviewerUserName))
            await SendRawAsync(HttpMethod.Post, $"repos/{Repo(repoRef)}/pulls/{details.MrId}/requested_reviewers", userToken,
                new { reviewers = new[] { reviewerUserName.Trim() } });

        return GitResult<MergeRequestDetailsDto>.Ok(details, created.StatusCode);
    }

    #endregion

    #region Review

    public async Task<List<ReviewMergeRequestDto>?> GetOpenReviewMergeRequestsAsync(string repoRef)
    {
        if (!IsValidRepoRef(repoRef))
            return Invalid<List<ReviewMergeRequestDto>>();

        var list = await GetAsync<JArray>($"repos/{Repo(repoRef)}/pulls?state=open&per_page=100&sort=updated&direction=desc");
        if (list == null)
            return null;

        var items = new List<ReviewMergeRequestDto>();
        foreach (var pull in list.OfType<JObject>())
        {
            var number = pull["number"]?.Value<int?>() ?? 0;
            var detail = items.Count < DetailLimit ? await GetAsync<JObject>($"repos/{Repo(repoRef)}/pulls/{number}") : null;
            items.Add(MapReview(detail ?? pull));
        }

        return items;
    }

    public async Task<List<ReviewMergeRequestDto>?> GetUserMergeRequestsAsync(GitUserMergeRequestQuery query, string userToken)
    {
        var login = await TokenLoginAsync(userToken);
        if (string.IsNullOrWhiteSpace(login))
            return null;

        var q = query.ReviewRequested
            ? $"is:pr is:open review-requested:{login}"
            : $"is:pr author:{login}{(query.OpenOnly ? " is:open" : string.Empty)}";

        var result = await SendAsync<JObject>(HttpMethod.Get, $"search/issues?q={Escape(q)}&per_page={Math.Min(query.Limit, 100)}&sort=updated&order=desc", userToken, personal: true);
        if (!result.Success || result.Data == null)
            return null;

        var items = new List<ReviewMergeRequestDto>();
        foreach (var item in (result.Data["items"] as JArray ?? []).OfType<JObject>())
        {
            var repoRef = RepoFromApiUrl(item["repository_url"]?.ToString());
            var number = item["number"]?.Value<int?>() ?? 0;
            var open = string.Equals(item["state"]?.ToString(), "open", StringComparison.OrdinalIgnoreCase);
            JObject? detail = null;
            if (open && repoRef != null && items.Count < 10)
            {
                var fetched = await SendAsync<JObject>(HttpMethod.Get, $"repos/{repoRef}/pulls/{number}", userToken, personal: true);
                detail = fetched.Data;
            }

            items.Add(detail != null ? MapReview(detail) : MapSearchItem(item));
        }

        return items;
    }

    public async Task<ReviewMergeRequestDto?> GetReviewMergeRequestAsync(string repoRef, int iid, string? userToken)
    {
        if (!IsValidRepoRef(repoRef))
            return Invalid<ReviewMergeRequestDto>();

        var pull = await GetAsync<JObject>($"repos/{Repo(repoRef)}/pulls/{iid}", userToken);
        if (pull == null)
            return null;

        var review = MapReview(pull);
        review.UserNotesCount = (pull["comments"]?.Value<int?>() ?? 0) + (pull["review_comments"]?.Value<int?>() ?? 0);
        return review;
    }

    public async Task<MergeRequestChangesDto?> GetChangesAsync(string repoRef, int iid, string? userToken)
    {
        if (!IsValidRepoRef(repoRef))
            return Invalid<MergeRequestChangesDto>();

        var files = await GetAsync<JArray>($"repos/{Repo(repoRef)}/pulls/{iid}/files?per_page=100", userToken);
        if (files == null)
            return null;

        return new MergeRequestChangesDto
        {
            WebUrl = $"{Context.WebBase}/{repoRef.Trim()}/pull/{iid}/files",
            Overflow = files.Count >= 100,
            Changes = files.OfType<JObject>().Select(f =>
            {
                var status = f["status"]?.ToString();
                var patch = f["patch"]?.ToString();
                return new MergeRequestFileChangeDto
                {
                    OldPath = f["previous_filename"]?.ToString() ?? f["filename"]?.ToString(),
                    NewPath = f["filename"]?.ToString(),
                    NewFile = status == "added",
                    DeletedFile = status == "removed",
                    RenamedFile = status == "renamed",
                    Diff = patch,
                    TooLarge = patch == null && status != "renamed"
                };
            }).ToList()
        };
    }

    public async Task<List<MergeRequestCommitDto>?> GetCommitsAsync(string repoRef, int iid, string? userToken)
    {
        if (!IsValidRepoRef(repoRef))
            return Invalid<List<MergeRequestCommitDto>>();

        var commits = await GetAsync<JArray>($"repos/{Repo(repoRef)}/pulls/{iid}/commits?per_page=100", userToken);
        return commits?.OfType<JObject>().Select(c =>
        {
            var sha = c["sha"]?.ToString() ?? string.Empty;
            var message = c["commit"]?["message"]?.ToString() ?? string.Empty;
            return new MergeRequestCommitDto
            {
                ShortId = sha.Length > 8 ? sha[..8] : sha,
                Title = message.Split('\n')[0],
                AuthorName = c["commit"]?["author"]?["name"]?.ToString(),
                CreatedAt = c["commit"]?["author"]?["date"]?.ToObject<DateTime?>() ?? DateTime.UtcNow,
                WebUrl = c["html_url"]?.ToString()
            };
        }).ToList();
    }

    public async Task<List<MergeRequestNoteDto>?> GetNotesAsync(string repoRef, int iid, string? userToken)
    {
        if (!IsValidRepoRef(repoRef))
            return Invalid<List<MergeRequestNoteDto>>();

        var comments = await GetAsync<JArray>($"repos/{Repo(repoRef)}/issues/{iid}/comments?per_page=100", userToken);
        return comments?.OfType<JObject>().Select(c => new MergeRequestNoteDto
        {
            Id = (int)(c["id"]?.Value<long?>() % int.MaxValue ?? 0),
            Body = c["body"]?.ToString(),
            Author = c["user"] is JObject user ? Person(user) : null,
            CreatedAt = c["created_at"]?.ToObject<DateTime?>() ?? DateTime.UtcNow,
            System = false
        }).ToList();
    }

    public async Task<MergeRequestApprovalsDto?> GetApprovalsAsync(string repoRef, int iid, string? userToken)
    {
        if (!IsValidRepoRef(repoRef))
            return Invalid<MergeRequestApprovalsDto>();

        var reviews = await GetAsync<JArray>($"repos/{Repo(repoRef)}/pulls/{iid}/reviews?per_page=100", userToken);
        if (reviews == null)
            return null;

        var latest = reviews.OfType<JObject>()
            .Where(r => r["user"] is JObject && r["state"]?.ToString() is "APPROVED" or "CHANGES_REQUESTED" or "DISMISSED")
            .GroupBy(r => r["user"]!["login"]?.ToString() ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.Last())
            .ToList();

        var approved = latest.Where(r => r["state"]?.ToString() == "APPROVED").ToList();
        var login = userToken == null ? null : await TokenLoginAsync(userToken);

        return new MergeRequestApprovalsDto
        {
            ApprovedBy = approved.Select(r => new MergeRequestApproverDto { User = Person((JObject)r["user"]!) }).ToList(),
            UserHasApproved = login != null && approved.Any(r => string.Equals(r["user"]?["login"]?.ToString(), login, StringComparison.OrdinalIgnoreCase)),
            UserCanApprove = true
        };
    }

    public async Task<GitResult<object>> ApproveAsync(string repoRef, int iid, string userToken)
    {
        if (!IsValidRepoRef(repoRef))
            return Remember(GitResult<object>.Fail("The repository reference is not valid."));

        return await SendAsync<object>(HttpMethod.Post, $"repos/{Repo(repoRef)}/pulls/{iid}/reviews", userToken, new { @event = "APPROVE" }, personal: true);
    }

    public Task<GitResult<object>> UnapproveAsync(string repoRef, int iid, string userToken)
    {
        return Task.FromResult(Remember(GitResult<object>.Fail("GitHub does not let you withdraw an approval. Request changes instead, or dismiss the review on GitHub.")));
    }

    public async Task<GitResult<object>> AddNoteAsync(string repoRef, int iid, string body, string userToken)
    {
        if (!IsValidRepoRef(repoRef))
            return Remember(GitResult<object>.Fail("The repository reference is not valid."));

        return await SendAsync<object>(HttpMethod.Post, $"repos/{Repo(repoRef)}/issues/{iid}/comments", userToken, new { body }, personal: true);
    }

    public async Task<GitResult<object>> RequestChangesAsync(string repoRef, int iid, string body, string userToken)
    {
        if (!IsValidRepoRef(repoRef))
            return Remember(GitResult<object>.Fail("The repository reference is not valid."));

        return await SendAsync<object>(HttpMethod.Post, $"repos/{Repo(repoRef)}/pulls/{iid}/reviews", userToken,
            new { @event = "REQUEST_CHANGES", body }, personal: true);
    }

    public async Task<GitResult<object>> MergeAsync(string repoRef, int iid, string? sha, bool squash, bool removeSourceBranch, string userToken)
    {
        if (!IsValidRepoRef(repoRef))
            return Remember(GitResult<object>.Fail("The repository reference is not valid."));

        var pull = await SendAsync<JObject>(HttpMethod.Get, $"repos/{Repo(repoRef)}/pulls/{iid}", userToken, personal: true);
        if (!pull.Success || pull.Data == null)
            return GitResult<object>.Fail(pull.Error ?? "Pull request not found.", pull.StatusCode);

        var body = new Dictionary<string, object> { ["merge_method"] = squash ? "squash" : "merge" };
        if (!string.IsNullOrWhiteSpace(sha))
            body["sha"] = sha;

        var merged = await SendAsync<object>(HttpMethod.Put, $"repos/{Repo(repoRef)}/pulls/{iid}/merge", userToken, body, personal: true);
        if (!merged.Success || !removeSourceBranch)
            return merged;

        var headRepo = pull.Data["head"]?["repo"]?["full_name"]?.ToString();
        var headRef = pull.Data["head"]?["ref"]?.ToString();
        if (!string.IsNullOrWhiteSpace(headRef) && string.Equals(headRepo, repoRef.Trim(), StringComparison.OrdinalIgnoreCase))
            await DeleteBranchAsync(repoRef, headRef, userToken);

        return merged;
    }

    public async Task<GitResult<object>> RebaseAsync(string repoRef, int iid, string userToken)
    {
        if (!IsValidRepoRef(repoRef))
            return Remember(GitResult<object>.Fail("The repository reference is not valid."));

        return await SendAsync<object>(HttpMethod.Put, $"repos/{Repo(repoRef)}/pulls/{iid}/update-branch", userToken, new { }, personal: true);
    }

    public async Task<GitResult<object>> CloseAsync(string repoRef, int iid, string userToken)
    {
        if (!IsValidRepoRef(repoRef))
            return Remember(GitResult<object>.Fail("The repository reference is not valid."));

        return await SendAsync<object>(HttpMethod.Patch, $"repos/{Repo(repoRef)}/pulls/{iid}", userToken, new { state = "closed" }, personal: true);
    }

    public async Task<GitResult<object>> SetReviewerAsync(string repoRef, int iid, GitPersonDto reviewer)
    {
        if (!IsValidRepoRef(repoRef) || string.IsNullOrWhiteSpace(reviewer.UserName))
            return Remember(GitResult<object>.Fail("The reviewer or repository reference is not valid."));

        return await SendAsync<object>(HttpMethod.Post, $"repos/{Repo(repoRef)}/pulls/{iid}/requested_reviewers",
            await ServiceTokenAsync($"repos/{Repo(repoRef)}"), new { reviewers = new[] { reviewer.UserName } });
    }

    #endregion

    #region Users

    public async Task<GitPersonDto?> FindUserAsync(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return null;

        var user = await GetAsync<JObject>($"users/{Escape(userName)}");
        return user == null ? null : Person(user);
    }

    public async Task<GitPersonDto?> GetUserByIdAsync(long id)
    {
        if (id <= 0)
            return null;

        var user = await GetAsync<JObject>($"user/{id}");
        return user == null ? null : Person(user);
    }

    public async Task<List<GitPersonDto>> SearchUsersAsync(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return [];

        var result = await GetAsync<JObject>($"search/users?q={Escape(query + " in:login")}&per_page=20");
        return (result?["items"] as JArray ?? []).OfType<JObject>().Select(Person).ToList();
    }

    public async Task<GitResult<GitTokenInfoDto>> ValidateUserTokenAsync(string userToken)
    {
        var response = await SendRawAsync(HttpMethod.Get, "user", userToken);
        if (response.StatusCode == 0)
            return GitResult<GitTokenInfoDto>.Fail($"Could not reach GitHub: {response.TransportError}");
        if (!response.Success)
            return GitResult<GitTokenInfoDto>.Fail(DescribeError(response.StatusCode, response.Content, true), response.StatusCode);

        var user = Deserialize<JObject>(response.Content);
        if (user == null)
            return GitResult<GitTokenInfoDto>.Fail("GitHub did not return the user for this token.");

        var scopes = (response.Header("X-OAuth-Scopes") ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        var info = new GitTokenInfoDto
        {
            ExternalUserId = user["id"]?.ToString() ?? string.Empty,
            UserName = user["login"]?.ToString() ?? string.Empty,
            DisplayName = user["name"]?.ToString(),
            Scopes = scopes
        };

        var expiration = response.Header("github-authentication-token-expiration");
        if (!string.IsNullOrWhiteSpace(expiration) && DateTime.TryParse(expiration.Replace(" UTC", "Z"), System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AdjustToUniversal, out var expires))
            info.ExpiresAt = expires;

        if (scopes.Count > 0 && !scopes.Contains("repo") && !scopes.Contains("public_repo"))
            info.Problem = $"This classic token only has the scope(s) {string.Join(", ", scopes)}. Taskist needs the 'repo' scope (or 'public_repo' for public repositories only).";

        return GitResult<GitTokenInfoDto>.Ok(info);
    }

    public async Task<GitResult<GitPersonDto>> GetServiceIdentityAsync()
    {
        if (Context.Connection.AuthMode == (int)GitAuthMode.GitHubApp)
        {
            if (Context.AppTokens == null || string.IsNullOrWhiteSpace(Context.AppPrivateKey) || string.IsNullOrWhiteSpace(Context.Connection.AppId))
                return GitResult<GitPersonDto>.Fail("The GitHub App id and private key are required.");

            string jwt;
            try
            {
                jwt = Context.AppTokens.CreateAppJwt(Context.Connection.AppId!, Context.AppPrivateKey!);
            }
            catch (Exception ex) when (ex is ArgumentException or System.Security.Cryptography.CryptographicException)
            {
                return GitResult<GitPersonDto>.Fail("The GitHub App private key could not be read. Paste the full PEM file contents.");
            }

            var app = await SendAsync<JObject>(HttpMethod.Get, "app", jwt);
            if (!app.Success || app.Data == null)
                return GitResult<GitPersonDto>.Fail(app.Error ?? "GitHub did not accept the app credentials.", app.StatusCode);

            if (app.Data["installations_count"]?.Value<int?>() == 0)
                return GitResult<GitPersonDto>.Fail("The GitHub App credentials are valid, but the app is not installed on any account yet. On GitHub open the app's settings → Install App, and install it on the account or organization that owns the repositories.");

            var slug = app.Data["slug"]?.ToString();
            return GitResult<GitPersonDto>.Ok(new GitPersonDto
            {
                Id = app.Data["id"]?.Value<long?>() ?? 0,
                UserName = $"{slug}[bot]",
                Name = app.Data["name"]?.ToString(),
                IsBot = true
            });
        }

        var user = await SendAsync<JObject>(HttpMethod.Get, "user", await ServiceTokenAsync(null));
        return user.Success && user.Data != null
            ? GitResult<GitPersonDto>.Ok(Person(user.Data))
            : GitResult<GitPersonDto>.Fail(user.Error ?? "GitHub did not accept the token.", user.StatusCode);
    }

    #endregion

    #region Issues

    public async Task<GitIssueDto?> GetIssueAsync(string repoRef, int number)
    {
        if (!IsValidRepoRef(repoRef))
            return Invalid<GitIssueDto>();

        var issue = await GetAsync<JObject>($"repos/{Repo(repoRef)}/issues/{number}");
        return issue == null || issue["pull_request"] != null ? null : MapIssue(issue);
    }

    public async Task<List<GitIssueDto>?> ListIssuesAsync(string repoRef, DateTime? updatedSince, string? label, int page)
    {
        if (!IsValidRepoRef(repoRef))
            return Invalid<List<GitIssueDto>>();

        var path = $"repos/{Repo(repoRef)}/issues?state=all&sort=updated&direction=asc&per_page=100&page={Math.Max(1, page)}";
        if (updatedSince != null)
            path += $"&since={Escape(updatedSince.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"))}";
        if (!string.IsNullOrWhiteSpace(label))
            path += $"&labels={Escape(label)}";

        var list = await GetAsync<JArray>(path);
        return list?.OfType<JObject>().Where(i => i["pull_request"] == null).Select(MapIssue).ToList();
    }

    public async Task<GitResult<GitIssueDto>> CreateIssueAsync(string repoRef, GitIssueWriteDto issue)
    {
        if (!IsValidRepoRef(repoRef))
            return Remember(GitResult<GitIssueDto>.Fail("The repository reference is not valid."));

        var result = await SendAsync<JObject>(HttpMethod.Post, $"repos/{Repo(repoRef)}/issues", await ServiceTokenAsync($"repos/{Repo(repoRef)}"), IssueBody(issue));
        return result.Success && result.Data != null
            ? GitResult<GitIssueDto>.Ok(MapIssue(result.Data))
            : GitResult<GitIssueDto>.Fail(result.Error ?? "GitHub did not create the issue.", result.StatusCode);
    }

    public async Task<GitResult<GitIssueDto>> UpdateIssueAsync(string repoRef, int number, GitIssueWriteDto issue)
    {
        if (!IsValidRepoRef(repoRef))
            return Remember(GitResult<GitIssueDto>.Fail("The repository reference is not valid."));

        var result = await SendAsync<JObject>(HttpMethod.Patch, $"repos/{Repo(repoRef)}/issues/{number}", await ServiceTokenAsync($"repos/{Repo(repoRef)}"), IssueBody(issue));
        return result.Success && result.Data != null
            ? GitResult<GitIssueDto>.Ok(MapIssue(result.Data))
            : GitResult<GitIssueDto>.Fail(result.Error ?? "GitHub did not update the issue.", result.StatusCode);
    }

    public async Task<List<GitIssueCommentDto>?> ListIssueCommentsAsync(string repoRef, int number)
    {
        if (!IsValidRepoRef(repoRef))
            return Invalid<List<GitIssueCommentDto>>();

        var list = await GetAsync<JArray>($"repos/{Repo(repoRef)}/issues/{number}/comments?per_page=100");
        return list?.OfType<JObject>().Select(MapComment).ToList();
    }

    public async Task<GitResult<GitIssueCommentDto>> CreateIssueCommentAsync(string repoRef, int number, string body)
    {
        if (!IsValidRepoRef(repoRef))
            return Remember(GitResult<GitIssueCommentDto>.Fail("The repository reference is not valid."));

        var result = await SendAsync<JObject>(HttpMethod.Post, $"repos/{Repo(repoRef)}/issues/{number}/comments", await ServiceTokenAsync($"repos/{Repo(repoRef)}"), new { body });
        return result.Success && result.Data != null
            ? GitResult<GitIssueCommentDto>.Ok(MapComment(result.Data))
            : GitResult<GitIssueCommentDto>.Fail(result.Error ?? "GitHub did not add the comment.", result.StatusCode);
    }

    public async Task<GitResult<GitIssueCommentDto>> UpdateIssueCommentAsync(string repoRef, int number, string commentId, string body)
    {
        if (!IsValidRepoRef(repoRef) || !long.TryParse(commentId, out var id))
            return Remember(GitResult<GitIssueCommentDto>.Fail("The comment reference is not valid."));

        var result = await SendAsync<JObject>(HttpMethod.Patch, $"repos/{Repo(repoRef)}/issues/comments/{id}", await ServiceTokenAsync($"repos/{Repo(repoRef)}"), new { body });
        return result.Success && result.Data != null
            ? GitResult<GitIssueCommentDto>.Ok(MapComment(result.Data))
            : GitResult<GitIssueCommentDto>.Fail(result.Error ?? "GitHub did not update the comment.", result.StatusCode);
    }

    #endregion

    #region OAuth

    public string BuildAuthorizeUrl(string redirectUri, string state, string codeChallenge)
    {
        return $"{Context.WebBase}/login/oauth/authorize?client_id={Escape(Context.Connection.OAuthClientId)}&redirect_uri={Escape(redirectUri)}" +
               $"&scope=repo&state={Escape(state)}&code_challenge={Escape(codeChallenge)}&code_challenge_method=S256&allow_signup=false";
    }

    public async Task<GitResult<GitOAuthTokenDto>> ExchangeCodeAsync(string code, string redirectUri, string codeVerifier)
    {
        return await TokenRequestAsync(new Dictionary<string, string>
        {
            ["client_id"] = Context.Connection.OAuthClientId ?? string.Empty,
            ["client_secret"] = Context.OAuthClientSecret ?? string.Empty,
            ["code"] = code,
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
        if (!oauth || !OAuthConfigured)
            return;

        await SendRawAsync(HttpMethod.Delete, $"applications/{Escape(Context.Connection.OAuthClientId)}/token", null,
            new { access_token = token },
            request => request.Headers.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Context.Connection.OAuthClientId}:{Context.OAuthClientSecret}"))));
    }

    #endregion

    #region Webhooks

    public async Task<GitResult<object>> RegisterWebhookAsync(string repoRef, GitWebhookRegistrationDto registration, string userToken)
    {
        if (!IsValidRepoRef(repoRef))
            return Remember(GitResult<object>.Fail("The repository reference is not valid."));

        var events = new List<string> { "push", "pull_request", "pull_request_review", "issue_comment" };
        if (registration.IssueEvents)
            events.Add("issues");

        return await SendAsync<object>(HttpMethod.Post, $"repos/{Repo(repoRef)}/hooks", userToken, new
        {
            name = "web",
            active = true,
            events,
            config = new { url = registration.Url, content_type = "json", secret = registration.Secret, insecure_ssl = "0" }
        }, personal: true);
    }

    #endregion

    #region Utilities

    protected override void ConfigureRequest(HttpRequestMessage request)
    {
        request.Headers.Accept.Clear();
        request.Headers.Accept.ParseAdd("application/vnd.github+json");
        request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
    }

    protected override async Task<string?> ServiceTokenAsync(string? path)
    {
        ServiceTokenError = null;
        if (Context.Connection.AuthMode != (int)GitAuthMode.GitHubApp)
            return Context.ServiceToken;

        if (Context.AppTokens == null || string.IsNullOrWhiteSpace(Context.Connection.AppId))
            return null;

        if (string.IsNullOrWhiteSpace(Context.AppPrivateKey))
        {
            ServiceTokenError = "The GitHub App private key is missing or could not be decrypted. Paste it again under Integrations → Git Connections.";
            return null;
        }

        string? owner = null;
        if (path != null && path.StartsWith("repos/", StringComparison.Ordinal))
            owner = Uri.UnescapeDataString(path["repos/".Length..].Split('/')[0]);

        try
        {
            var result = await Context.AppTokens.GetInstallationTokenAsync(Context.Connection.Id, Context.Connection.AppId!, Context.AppPrivateKey!,
                Context.ApiBase, Context.Http, owner);
            if (!result.Success)
                ServiceTokenError = result.Error;

            return result.Data;
        }
        catch (Exception ex) when (ex is ArgumentException or System.Security.Cryptography.CryptographicException or Newtonsoft.Json.JsonException)
        {
            ServiceTokenError = "The GitHub App private key could not be read. Paste the full PEM file contents under Integrations → Git Connections.";
            return null;
        }
        catch (HttpRequestException ex)
        {
            ServiceTokenError = $"Could not reach GitHub to get a GitHub App installation token: {ex.Message}";
            return null;
        }
    }

    private T? Invalid<T>() where T : class
    {
        LastError = "The repository reference is not valid. Use the form owner/repository.";
        return null;
    }

    private static string Repo(string repoRef)
    {
        var parts = repoRef.Trim().Split('/');
        return $"{Uri.EscapeDataString(parts[0])}/{Uri.EscapeDataString(parts[1])}";
    }

    private static string EscapePath(string value) => string.Join("/", value.Trim().Split('/').Select(Uri.EscapeDataString));

    private async Task<string?> DefaultBranchAsync(string repoRef)
    {
        if (_defaultBranches.TryGetValue(repoRef, out var known))
            return known;

        var repo = await GetAsync<JObject>($"repos/{Repo(repoRef)}");
        var branch = repo?["default_branch"]?.ToString();
        _defaultBranches[repoRef] = branch;
        return branch;
    }

    private async Task<string?> TokenLoginAsync(string userToken)
    {
        var key = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(userToken)));
        if (_tokenLogins.TryGetValue(key, out var known))
            return known;

        var user = await SendAsync<JObject>(HttpMethod.Get, "user", userToken, personal: true);
        var login = user.Data?["login"]?.ToString();
        _tokenLogins[key] = login;
        return login;
    }

    private async Task<GitResult<GitOAuthTokenDto>> TokenRequestAsync(Dictionary<string, string> form)
    {
        if (!OAuthConfigured)
            return GitResult<GitOAuthTokenDto>.Fail("OAuth is not configured for this connection.");

        var response = await SendRawAsync(HttpMethod.Post, $"{Context.WebBase}/login/oauth/access_token", null, new FormUrlEncodedContent(form),
            request =>
            {
                request.Headers.Accept.Clear();
                request.Headers.Accept.ParseAdd("application/json");
                request.Headers.Remove("X-GitHub-Api-Version");
            }, absolute: true);

        var json = Deserialize<JObject>(response.Content);
        var accessToken = json?["access_token"]?.ToString();
        if (!response.Success || string.IsNullOrWhiteSpace(accessToken))
            return GitResult<GitOAuthTokenDto>.Fail(json?["error_description"]?.ToString() ?? DescribeError(response.StatusCode, response.Content, true), response.StatusCode);

        var expiresIn = json?["expires_in"]?.Value<long?>();
        var refreshExpiresIn = json?["refresh_token_expires_in"]?.Value<long?>();
        return GitResult<GitOAuthTokenDto>.Ok(new GitOAuthTokenDto
        {
            AccessToken = accessToken,
            RefreshToken = json?["refresh_token"]?.ToString(),
            ExpiresOn = expiresIn > 0 ? DateTime.UtcNow.AddSeconds(expiresIn.Value) : null,
            RefreshExpiresOn = refreshExpiresIn > 0 ? DateTime.UtcNow.AddSeconds(refreshExpiresIn.Value) : null,
            Scopes = json?["scope"]?.ToString()
        });
    }

    private static object IssueBody(GitIssueWriteDto issue)
    {
        var body = new Dictionary<string, object?>();
        if (issue.Title != null)
            body["title"] = issue.Title;
        if (issue.Body != null)
            body["body"] = issue.Body;
        if (issue.Labels != null)
            body["labels"] = issue.Labels;
        if (issue.Assignees != null)
            body["assignees"] = issue.Assignees.Where(a => !string.IsNullOrWhiteSpace(a.UserName)).Select(a => a.UserName).ToArray();
        if (issue.Closed != null)
            body["state"] = issue.Closed.Value ? "closed" : "open";
        return body;
    }

    private static string? RepoFromApiUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        var marker = url.IndexOf("/repos/", StringComparison.Ordinal);
        return marker < 0 ? null : url[(marker + "/repos/".Length)..].Trim('/');
    }

    private static GitRepositoryDto MapRepository(JObject repo)
    {
        return new GitRepositoryDto
        {
            Id = repo["id"]?.Value<long?>() ?? 0,
            Name = repo["name"]?.ToString() ?? string.Empty,
            PathWithNamespace = repo["full_name"]?.ToString() ?? string.Empty,
            DefaultBranch = repo["default_branch"]?.ToString(),
            WebUrl = repo["html_url"]?.ToString(),
            Visibility = repo["visibility"]?.ToString() ?? (repo["private"]?.Value<bool?>() == false ? "public" : "private")
        };
    }

    private static string PullState(JObject pull)
    {
        if (pull["merged_at"] != null && pull["merged_at"]!.Type != JTokenType.Null)
            return "merged";
        if (pull["merged"]?.Type == JTokenType.Boolean && pull["merged"]!.Value<bool>())
            return "merged";
        return string.Equals(pull["state"]?.ToString(), "open", StringComparison.OrdinalIgnoreCase) ? "opened" : "closed";
    }

    private static MergeRequestDetailsDto MapDetails(JObject pull)
    {
        return new MergeRequestDetailsDto
        {
            MrId = pull["number"]?.Value<int?>() ?? 0,
            Title = pull["title"]?.ToString() ?? string.Empty,
            Description = pull["body"]?.Type == JTokenType.Null ? string.Empty : pull["body"]?.ToString() ?? string.Empty,
            State = PullState(pull),
            CreatedAt = pull["created_at"]?.ToObject<DateTime?>() ?? default,
            UpdatedAt = pull["updated_at"]?.ToObject<DateTime?>() ?? default,
            MergedAt = pull["merged_at"]?.ToObject<DateTime?>(),
            ClosedAt = pull["closed_at"]?.ToObject<DateTime?>(),
            TargetBranch = pull["base"]?["ref"]?.ToString() ?? string.Empty,
            SourceBranch = pull["head"]?["ref"]?.ToString() ?? string.Empty,
            Author = pull["user"] is JObject user ? Person(user) : new GitPersonDto(),
            Draft = pull["draft"]?.Value<bool?>() ?? false,
            Url = pull["html_url"]?.ToString() ?? string.Empty
        };
    }

    private static string? DetailedStatus(JObject pull)
    {
        if (pull["draft"]?.Value<bool?>() == true)
            return "draft_status";

        return pull["mergeable_state"]?.ToString()?.ToLowerInvariant() switch
        {
            "clean" or "unstable" or "has_hooks" => "mergeable",
            "dirty" => "conflict",
            "behind" => "need_rebase",
            "blocked" => "not_approved",
            "draft" => "draft_status",
            "unknown" => "checking",
            null => null,
            _ => "checking"
        };
    }

    private static ReviewMergeRequestDto MapReview(JObject pull)
    {
        var detailed = DetailedStatus(pull);
        return new ReviewMergeRequestDto
        {
            Iid = pull["number"]?.Value<int?>() ?? 0,
            ProjectId = pull["base"]?["repo"]?["id"]?.Value<long?>() ?? 0,
            Title = pull["title"]?.ToString(),
            Description = pull["body"]?.Type == JTokenType.Null ? null : pull["body"]?.ToString(),
            State = PullState(pull),
            Draft = pull["draft"]?.Value<bool?>() ?? false,
            DetailedMergeStatus = detailed ?? "checking",
            HasConflicts = pull["mergeable"]?.Type == JTokenType.Boolean && !pull["mergeable"]!.Value<bool>() && detailed == "conflict",
            SourceBranch = pull["head"]?["ref"]?.ToString(),
            TargetBranch = pull["base"]?["ref"]?.ToString(),
            SourceProjectId = pull["head"]?["repo"]?["id"]?.Value<long?>() ?? 0,
            TargetProjectId = pull["base"]?["repo"]?["id"]?.Value<long?>() ?? 0,
            Author = pull["user"] is JObject user ? Person(user) : null,
            Reviewers = (pull["requested_reviewers"] as JArray ?? []).OfType<JObject>().Select(Person).ToList(),
            Assignees = (pull["assignees"] as JArray ?? []).OfType<JObject>().Select(Person).ToList(),
            CreatedAt = pull["created_at"]?.ToObject<DateTime?>() ?? default,
            UpdatedAt = pull["updated_at"]?.ToObject<DateTime?>() ?? default,
            WebUrl = pull["html_url"]?.ToString(),
            UserNotesCount = (pull["comments"]?.Value<int?>() ?? 0) + (pull["review_comments"]?.Value<int?>() ?? 0),
            Sha = pull["head"]?["sha"]?.ToString(),
            ChangesCount = pull["changed_files"]?.ToString(),
            ForceRemoveSourceBranch = null
        };
    }

    private static ReviewMergeRequestDto MapSearchItem(JObject item)
    {
        var merged = item["pull_request"]?["merged_at"] is JToken mergedAt && mergedAt.Type != JTokenType.Null;
        var open = string.Equals(item["state"]?.ToString(), "open", StringComparison.OrdinalIgnoreCase);
        return new ReviewMergeRequestDto
        {
            Iid = item["number"]?.Value<int?>() ?? 0,
            Title = item["title"]?.ToString(),
            State = merged ? "merged" : open ? "opened" : "closed",
            Draft = item["draft"]?.Value<bool?>() ?? false,
            DetailedMergeStatus = "checking",
            Author = item["user"] is JObject user ? Person(user) : null,
            CreatedAt = item["created_at"]?.ToObject<DateTime?>() ?? default,
            UpdatedAt = item["updated_at"]?.ToObject<DateTime?>() ?? default,
            WebUrl = item["html_url"]?.ToString(),
            UserNotesCount = item["comments"]?.Value<int?>() ?? 0
        };
    }

    public static GitIssueDto MapIssue(JObject issue)
    {
        return new GitIssueDto
        {
            ExternalId = issue["id"]?.ToString() ?? string.Empty,
            Number = issue["number"]?.Value<int?>() ?? 0,
            Title = issue["title"]?.ToString() ?? string.Empty,
            Body = issue["body"]?.Type == JTokenType.Null ? null : issue["body"]?.ToString(),
            Closed = string.Equals(issue["state"]?.ToString(), "closed", StringComparison.OrdinalIgnoreCase),
            Labels = (issue["labels"] as JArray ?? [])
                .Select(l => l.Type == JTokenType.String ? l.ToString() : l["name"]?.ToString())
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(l => l!)
                .ToList(),
            Assignees = (issue["assignees"] as JArray ?? []).OfType<JObject>().Select(Person).ToList(),
            Author = issue["user"] is JObject user ? Person(user) : null,
            Url = issue["html_url"]?.ToString(),
            CreatedAt = issue["created_at"]?.ToObject<DateTime?>() ?? DateTime.UtcNow,
            UpdatedAt = issue["updated_at"]?.ToObject<DateTime?>() ?? DateTime.UtcNow
        };
    }

    public static GitIssueCommentDto MapComment(JObject comment)
    {
        return new GitIssueCommentDto
        {
            ExternalId = comment["id"]?.ToString() ?? string.Empty,
            Body = comment["body"]?.ToString(),
            Author = comment["user"] is JObject user ? Person(user) : null,
            Url = comment["html_url"]?.ToString(),
            CreatedAt = comment["created_at"]?.ToObject<DateTime?>() ?? DateTime.UtcNow,
            UpdatedAt = comment["updated_at"]?.ToObject<DateTime?>() ?? DateTime.UtcNow
        };
    }

    public static GitPersonDto Person(JObject user)
    {
        return new GitPersonDto
        {
            Id = user["id"]?.Value<long?>() ?? 0,
            UserName = user["login"]?.ToString(),
            Name = user["name"]?.ToString(),
            AvatarUrl = user["avatar_url"]?.ToString(),
            WebUrl = user["html_url"]?.ToString(),
            Email = user["email"]?.Type == JTokenType.Null ? null : user["email"]?.ToString(),
            IsBot = string.Equals(user["type"]?.ToString(), "Bot", StringComparison.OrdinalIgnoreCase)
        };
    }

    #endregion
}
