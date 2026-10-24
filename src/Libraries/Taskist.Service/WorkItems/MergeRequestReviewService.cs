using Microsoft.EntityFrameworkCore;
using Taskist.Core.Caching;
using Taskist.Core.Domain.Masters;
using Taskist.Core.Domain.Settings;
using Taskist.Core.Domain.Users;
using Taskist.Core.Domain.WorkItems;
using Taskist.Core.Dtos.Git;
using Taskist.Data.Repository;
using Taskist.Service.Git;

namespace Taskist.Service.WorkItems;

public class MergeRequestReviewService : IMergeRequestReviewService
{
    #region Fields

    private const string QueueCacheKey = "wc.review.queue.r{0}";
    private const int QueueCacheMinutes = 1;

    private readonly IRepository<Project> _projectRepository;
    private readonly IRepository<UserProjectMap> _userProjectMapRepository;
    private readonly IRepository<Backlog> _backlogRepository;
    private readonly IRepository<BacklogComment> _backlogCommentRepository;
    private readonly IGitRepositoryService _gitRepositoryService;
    private readonly IGitAccountService _gitAccountService;
    private readonly ITaskCodeService _taskCodeService;
    private readonly ICacheManager _cacheManager;
    private readonly IntegrationSettings _integrationSettings;
    private readonly ICodeReviewNotificationService _codeReviewNotificationService;

    #endregion

    #region Ctor

    public MergeRequestReviewService(IRepository<Project> projectRepository,
        IRepository<UserProjectMap> userProjectMapRepository,
        IRepository<Backlog> backlogRepository,
        IRepository<BacklogComment> backlogCommentRepository,
        IGitRepositoryService gitRepositoryService,
        IGitAccountService gitAccountService,
        ITaskCodeService taskCodeService,
        ICacheManager cacheManager,
        IntegrationSettings integrationSettings,
        ICodeReviewNotificationService codeReviewNotificationService)
    {
        _projectRepository = projectRepository;
        _userProjectMapRepository = userProjectMapRepository;
        _backlogRepository = backlogRepository;
        _backlogCommentRepository = backlogCommentRepository;
        _gitRepositoryService = gitRepositoryService;
        _gitAccountService = gitAccountService;
        _taskCodeService = taskCodeService;
        _cacheManager = cacheManager;
        _integrationSettings = integrationSettings;
        _codeReviewNotificationService = codeReviewNotificationService;
    }

    #endregion

    #region Methods

    public async Task<IList<ReviewProjectScopeDto>> GetScopeAsync(User user)
    {
        if (user == null)
            return [];

        var maps = await _userProjectMapRepository.Table
            .Where(x => x.UserId == user.Id && (x.CanReviewMergeRequests || x.CanMergeMergeRequests))
            .Select(x => new { x.ProjectId, x.CanReviewMergeRequests, x.CanMergeMergeRequests })
            .ToListAsync();
        var mappedIds = maps.Select(x => x.ProjectId).ToList();

        var projects = await _projectRepository.Table
            .Where(x => !x.Deleted && x.Active
                        && (x.TeamLeadId == user.Id || x.CodeReviewerId == user.Id || mappedIds.Contains(x.Id)))
            .OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name, x.TeamLeadId, x.CodeReviewerId })
            .ToListAsync();

        var repositories = await _gitRepositoryService.GetRepositoriesForProjectsAsync(projects.Select(p => p.Id).ToList());
        var result = new List<ReviewProjectScopeDto>();

        foreach (var p in projects)
        {
            var map = maps.FirstOrDefault(m => m.ProjectId == p.Id);
            var isLead = p.TeamLeadId == user.Id;
            var isCodeReviewer = p.CodeReviewerId == user.Id;

            foreach (var repository in repositories.Where(r => r.ProjectId == p.Id))
            {
                var context = await _gitRepositoryService.GetContextAsync(repository);
                if (context == null)
                    continue;

                result.Add(new ReviewProjectScopeDto
                {
                    ProjectId = p.Id,
                    ProjectName = p.Name,
                    RepositoryId = context.RepositoryId,
                    RepositoryName = context.Name,
                    ConnectionId = context.Connection.Id,
                    ProviderType = context.Connection.ProviderType,
                    GitProjectReference = context.RepoRef,
                    IsTeamLead = isLead,
                    IsCodeReviewer = isCodeReviewer,
                    CanReview = isLead || isCodeReviewer || (map?.CanReviewMergeRequests ?? false) || (map?.CanMergeMergeRequests ?? false),
                    CanMerge = isLead || isCodeReviewer || (map?.CanMergeMergeRequests ?? false),
                    CanRebase = context.Capabilities.CanRebase,
                    Term = context.Capabilities.Term,
                    ShortTerm = context.Capabilities.ShortTerm,
                    RefPrefix = context.Capabilities.RefPrefix
                });
            }
        }

        return result;
    }

    public async Task<ReviewProjectScopeDto?> GetRepositoryScopeAsync(User user, int repositoryId)
    {
        return (await GetScopeAsync(user)).FirstOrDefault(x => x.RepositoryId == repositoryId);
    }

    public async Task<bool> CanReviewProjectAsync(User user, int projectId)
    {
        return (await GetScopeAsync(user)).Any(x => x.ProjectId == projectId);
    }

    public async Task<ReviewQueueDto> GetQueueAsync(User user, bool refresh = false)
    {
        var result = new ReviewQueueDto();
        if (!await _gitRepositoryService.AnyConfiguredAsync())
        {
            result.Error = "No Git connection is configured. An administrator can add one under Configuration → Git connections.";
            return result;
        }

        result.Projects = (await GetScopeAsync(user)).ToList();
        var userNames = new Dictionary<int, string?>();

        foreach (var scope in result.Projects)
        {
            var context = await _gitRepositoryService.GetContextAsync(scope.RepositoryId);
            if (context == null)
                continue;

            if (!userNames.TryGetValue(scope.ConnectionId, out var gitUser))
            {
                gitUser = await _gitAccountService.GetUserNameAsync(user, context.Connection);
                userNames[scope.ConnectionId] = gitUser;
            }

            var key = string.Format(QueueCacheKey, scope.RepositoryId);
            if (refresh)
                await _cacheManager.RemoveAsync(key);

            var items = await _cacheManager.GetAsync(key, () => LoadQueueAsync(scope, context), QueueCacheMinutes);
            if (items == null)
            {
                await _cacheManager.RemoveAsync(key);
                result.FailedProjects.Add(scope.DisplayName);
                result.Error = context.Provider.LastError;
                continue;
            }

            foreach (var source in items)
            {
                var item = Clone(source);
                item.IsLeadProject = scope.IsTeamLead || scope.IsCodeReviewer;
                item.IsOwn = Same(item.AuthorUserName, gitUser);
                item.IsMine = !item.IsOwn && (item.ReviewerUserNames.Any(r => Same(r, gitUser))
                                              || item.AssigneeUserNames.Any(a => Same(a, gitUser))
                                              || item.IsLeadProject);
                result.Items.Add(item);
            }
        }

        result.Items = result.Items.OrderByDescending(x => x.UpdatedAt).ToList();
        return result;
    }

    public async Task<(ReviewDetailDto? Detail, string? Error)> GetDetailAsync(User user, int repositoryId, int iid)
    {
        var (scope, context) = await ScopeAsync(user, repositoryId);
        if (scope == null || context == null)
            return (null, "You are not a reviewer for this repository.");

        var provider = context.Provider;
        var token = await _gitAccountService.GetTokenAsync(user, context.Connection);
        var mr = await provider.GetReviewMergeRequestAsync(context.RepoRef, iid, token);
        if (mr == null)
            return (null, provider.LastError ?? $"The {scope.Term} was not found.");

        var userName = await _gitAccountService.GetUserNameAsync(user, context.Connection);
        var detail = new ReviewDetailDto
        {
            Scope = scope,
            MergeRequest = mr,
            Status = ReviewMergeStatus.From(mr),
            Connected = !string.IsNullOrWhiteSpace(token),
            IsOwn = Same(mr.Author?.UserName, userName)
        };

        var approvals = await provider.GetApprovalsAsync(context.RepoRef, iid, token);
        if (approvals != null)
        {
            detail.ApprovedBy = approvals.ApprovedBy.Select(a => a.User?.Name ?? a.User?.UserName ?? "?").ToList();
            detail.UserHasApproved = approvals.UserHasApproved;
        }

        detail.Commits = await provider.GetCommitsAsync(context.RepoRef, iid, token) ?? [];
        detail.Notes = (await provider.GetNotesAsync(context.RepoRef, iid, token) ?? [])
            .Where(n => !n.System)
            .ToList();
        detail.Tasks = await LoadTasksAsync(context, mr);
        detail.TitleIsTaskReference = detail.Tasks.Count > 0 && IsOnlyTaskReferences(mr.Title);
        detail.CanUnapprove = context.Capabilities.CanUnapprove;
        detail.RebaseLabel = context.Capabilities.RebaseLabel;
        detail.MergeBlockedReason = BlockedReason(detail);

        return (detail, null);
    }

    public async Task<(MergeRequestChangesDto? Changes, string? Error)> GetChangesAsync(User user, int repositoryId, int iid)
    {
        var (scope, context) = await ScopeAsync(user, repositoryId);
        if (scope == null || context == null)
            return (null, "You are not a reviewer for this repository.");

        var token = await _gitAccountService.GetTokenAsync(user, context.Connection);
        var changes = await context.Provider.GetChangesAsync(context.RepoRef, iid, token);
        return changes == null ? (null, context.Provider.LastError ?? "Could not load the changes.") : (changes, null);
    }

    public async Task<ReviewActionResult> SetApprovalAsync(User user, int repositoryId, int iid, bool approve)
    {
        var (scope, context, token, error) = await PrepareAsync(user, repositoryId, requireMerge: false);
        if (error != null)
            return ReviewActionResult.Fail(error);

        var provider = context!.Provider;
        var result = approve
            ? await provider.ApproveAsync(context.RepoRef, iid, token!)
            : await provider.UnapproveAsync(context.RepoRef, iid, token!);
        if (!result.Success)
            return ReviewActionResult.Fail(result.Error ?? "The request was not accepted.");

        var label = context.Capabilities.Ref(iid);
        if (approve)
        {
            var approved = await provider.GetReviewMergeRequestAsync(context.RepoRef, iid, token);
            if (approved != null)
                await _codeReviewNotificationService.ApprovedAsync(scope!.ProjectId, ToRef(approved, context),
                    await _gitAccountService.GetUserNameAsync(user, context.Connection), user.Name);
        }

        await ClearAsync(context);
        return ReviewActionResult.Ok(approve ? $"Approved {label}." : $"Approval removed from {label}.");
    }

    public async Task<ReviewActionResult> CommentAsync(User user, int repositoryId, int iid, string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return ReviewActionResult.Fail("Write a comment first.");
        if (body.Length > 20000)
            return ReviewActionResult.Fail("The comment is too long.");

        var (_, context, token, error) = await PrepareAsync(user, repositoryId, requireMerge: false);
        if (error != null)
            return ReviewActionResult.Fail(error);

        var result = await context!.Provider.AddNoteAsync(context.RepoRef, iid, body.Trim(), token!);
        if (!result.Success)
            return ReviewActionResult.Fail(result.Error ?? "The comment was not accepted.");

        await ClearAsync(context);
        return ReviewActionResult.Ok("Comment added.");
    }

    public async Task<ReviewActionResult> RejectAsync(User user, int repositoryId, int iid, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return ReviewActionResult.Fail("Give a reason for rejecting it.");

        var (scope, context, token, error) = await PrepareAsync(user, repositoryId, requireMerge: false);
        if (error != null)
            return ReviewActionResult.Fail(error);

        var provider = context!.Provider;
        var label = context.Capabilities.Label(iid);
        var mr = await provider.GetReviewMergeRequestAsync(context.RepoRef, iid, token);
        if (mr == null)
            return ReviewActionResult.Fail(provider.LastError ?? $"The {scope!.Term} was not found.");
        if (mr.State != "opened")
            return ReviewActionResult.Fail($"{label} is already {mr.State}.");

        var note = await provider.AddNoteAsync(context.RepoRef, iid, $"**Rejected** in Taskist by {user.Name}:\n\n{reason.Trim()}", token!);
        if (!note.Success)
            return ReviewActionResult.Fail(note.Error ?? "The rejection comment was not accepted.");

        var closed = await provider.CloseAsync(context.RepoRef, iid, token!);
        if (!closed.Success)
            return ReviewActionResult.Fail(closed.Error ?? $"The {scope!.Term} was not closed.");

        var tasks = await LoadTasksAsync(context, mr);
        if (tasks.Count > 0)
        {
            await _backlogCommentRepository.InsertAsync(tasks.Select(t => new BacklogComment
            {
                BacklogId = t.Id,
                CreatedById = user.Id,
                CreatedOn = DateTime.UtcNow,
                Comment = $"{label} rejected by {user.Name}: {reason.Trim()}",
                SystemComment = true
            }).ToList());
        }

        await _codeReviewNotificationService.RejectedAsync(scope!.ProjectId, ToRef(mr, context),
            await _gitAccountService.GetUserNameAsync(user, context.Connection), user.Name, reason.Trim());

        await ClearAsync(context, tasks.Select(t => t.Id));
        return ReviewActionResult.Ok($"{label} rejected and closed.");
    }

    public async Task<ReviewActionResult> RequestChangesAsync(User user, int repositoryId, int iid, string notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
            return ReviewActionResult.Fail("Describe what needs to change.");

        var (scope, context, token, error) = await PrepareAsync(user, repositoryId, requireMerge: false);
        if (error != null)
            return ReviewActionResult.Fail(error);

        var provider = context!.Provider;
        var label = context.Capabilities.Label(iid);
        var mr = await provider.GetReviewMergeRequestAsync(context.RepoRef, iid, token);
        if (mr == null)
            return ReviewActionResult.Fail(provider.LastError ?? $"The {scope!.Term} was not found.");
        if (mr.State != "opened")
            return ReviewActionResult.Fail($"{label} is already {mr.State}.");

        var requested = await provider.RequestChangesAsync(context.RepoRef, iid, $"**Changes requested** in Taskist by {user.Name}:\n\n{notes.Trim()}", token!);
        if (!requested.Success)
            return ReviewActionResult.Fail(requested.Error ?? "The request was not accepted.");

        var tasks = await LoadTasksAsync(context, mr);
        if (tasks.Count > 0)
        {
            await _backlogCommentRepository.InsertAsync(tasks.Select(t => new BacklogComment
            {
                BacklogId = t.Id,
                CreatedById = user.Id,
                CreatedOn = DateTime.UtcNow,
                Comment = $"Changes requested on {label} by {user.Name}: {notes.Trim()}",
                SystemComment = true
            }).ToList());
        }

        await _codeReviewNotificationService.ChangesRequestedAsync(scope!.ProjectId, ToRef(mr, context),
            await _gitAccountService.GetUserNameAsync(user, context.Connection), user.Name, notes.Trim());

        await ClearAsync(context, tasks.Select(t => t.Id));
        return ReviewActionResult.Ok($"Changes requested on {label}. It stays open.");
    }

    public async Task<ReviewActionResult> MergeAsync(User user, int repositoryId, int iid, string? sha, bool squash, bool removeSourceBranch)
    {
        var (scope, context, token, error) = await PrepareAsync(user, repositoryId, requireMerge: true);
        if (error != null)
            return ReviewActionResult.Fail(error);

        var provider = context!.Provider;
        var mr = await provider.GetReviewMergeRequestAsync(context.RepoRef, iid, token);
        if (mr == null)
            return ReviewActionResult.Fail(provider.LastError ?? $"The {scope!.Term} was not found.");

        var status = ReviewMergeStatus.From(mr);
        if (mr.State != "opened" || status != ReviewMergeStatus.Mergeable)
            return ReviewActionResult.Fail(BlockedReason(new ReviewDetailDto { Scope = scope!, MergeRequest = mr, Status = status, Connected = true })
                                           ?? $"The {scope!.Term} is not reported as ready to merge.");

        var result = await provider.MergeAsync(context.RepoRef, iid, sha, squash, removeSourceBranch, token!);
        if (!result.Success)
            return ReviewActionResult.Fail(result.Error ?? "It was not merged.");

        var tasks = await LoadTasksAsync(context, mr);
        await ClearAsync(context, tasks.Select(t => t.Id));
        return ReviewActionResult.Ok($"{context.Capabilities.Label(iid)} merged into {mr.TargetBranch}.");
    }

    public async Task<ReviewActionResult> RebaseAsync(User user, int repositoryId, int iid)
    {
        var (_, context, token, error) = await PrepareAsync(user, repositoryId, requireMerge: true);
        if (error != null)
            return ReviewActionResult.Fail(error);

        if (!context!.Capabilities.CanRebase)
            return ReviewActionResult.Fail("This provider does not support it.");

        var result = await context.Provider.RebaseAsync(context.RepoRef, iid, token!);
        if (!result.Success)
            return ReviewActionResult.Fail(result.Error ?? "It was not started.");

        await ClearAsync(context);
        return ReviewActionResult.Ok($"{context.Capabilities.RebaseLabel} of {context.Capabilities.Ref(iid)} started. Refresh in a moment to see the result.");
    }

    public async Task<(int Conflicting, int Notified)> CheckConflictsAsync(User user)
    {
        var queue = await GetQueueAsync(user, refresh: true);
        var conflicting = queue.Items
            .Where(x => x.Status == ReviewMergeStatus.Conflict || x.Status == ReviewMergeStatus.NeedsRebase)
            .ToList();

        var notified = 0;
        foreach (var item in conflicting)
        {
            var scope = queue.Projects.FirstOrDefault(p => p.RepositoryId == item.RepositoryId);
            var sent = await _codeReviewNotificationService.ConflictAsync(item.ProjectId, item.ProjectName, new MergeRequestRefDto
            {
                Iid = item.Iid,
                Title = item.Title,
                WebUrl = item.WebUrl,
                AuthorUserName = item.AuthorUserName,
                Sha = item.Sha,
                ConnectionId = scope?.ConnectionId ?? 0,
                RepositoryId = item.RepositoryId,
                RefPrefix = item.RefPrefix
            });
            if (sent)
                notified++;
        }

        return (conflicting.Count, notified);
    }

    public async Task AutoAssignReviewerAsync(GitRepositoryContext repository, int iid, long authorExternalId)
    {
        if (!_integrationSettings.AutoAssignTeamLeadReviewer || iid <= 0 || !repository.Provider.IsConfigured)
            return;

        var project = await _projectRepository.GetByIdAsync(repository.Repository.ProjectId);
        if (project == null)
            return;

        var provider = repository.Provider;
        var mr = await provider.GetReviewMergeRequestAsync(repository.RepoRef, iid, null);
        if (mr == null || mr.Reviewers.Count > 0)
            return;

        var authorId = authorExternalId > 0 ? authorExternalId : mr.Author?.Id ?? 0;
        foreach (var candidate in new[] { project.TeamLead, project.CodeReviewer })
        {
            var userName = await _gitAccountService.GetUserNameAsync(candidate, repository.Connection);
            if (string.IsNullOrWhiteSpace(userName))
                continue;

            var person = await provider.FindUserAsync(userName);
            if (person == null || person.Id == authorId)
                continue;

            var assigned = await provider.SetReviewerAsync(repository.RepoRef, iid, person);
            await ClearQueueCacheAsync(repository.RepositoryId);
            if (assigned.Success)
                await _codeReviewNotificationService.ReviewerAssignedAsync(project.Id, ToRef(mr, repository), [person.UserName ?? userName], null);
            return;
        }
    }

    public async Task ClearQueueCacheAsync(int repositoryId)
    {
        await _cacheManager.RemoveAsync(string.Format(QueueCacheKey, repositoryId));
    }

    #endregion

    #region Utilities

    private async Task<(ReviewProjectScopeDto? Scope, GitRepositoryContext? Context)> ScopeAsync(User user, int repositoryId)
    {
        var scope = await GetRepositoryScopeAsync(user, repositoryId);
        if (scope == null)
            return (null, null);

        return (scope, await _gitRepositoryService.GetContextAsync(repositoryId));
    }

    private async Task<List<ReviewQueueItemDto>?> LoadQueueAsync(ReviewProjectScopeDto scope, GitRepositoryContext context)
    {
        var mergeRequests = await context.Provider.GetOpenReviewMergeRequestsAsync(context.RepoRef);
        if (mergeRequests == null)
            return null;

        var items = new List<ReviewQueueItemDto>();
        foreach (var mr in mergeRequests)
        {
            items.Add(new ReviewQueueItemDto
            {
                ProjectId = scope.ProjectId,
                ProjectName = scope.DisplayName,
                RepositoryId = scope.RepositoryId,
                RefPrefix = scope.RefPrefix,
                Iid = mr.Iid,
                Title = mr.Title,
                AuthorName = mr.Author?.Name ?? mr.Author?.UserName,
                AuthorUserName = mr.Author?.UserName,
                ReviewerUserNames = mr.Reviewers.Select(r => r.UserName ?? string.Empty).ToList(),
                ReviewerNames = mr.Reviewers.Select(r => r.Name ?? r.UserName ?? string.Empty).ToList(),
                AssigneeUserNames = mr.Assignees.Select(a => a.UserName ?? string.Empty).ToList(),
                CreatedAt = mr.CreatedAt,
                UpdatedAt = mr.UpdatedAt,
                Status = ReviewMergeStatus.From(mr),
                SourceBranch = mr.SourceBranch,
                TargetBranch = mr.TargetBranch,
                WebUrl = mr.WebUrl,
                NotesCount = mr.UserNotesCount,
                Sha = mr.Sha,
                TaskIds = (await _taskCodeService.ResolveTaskIdsAsync(scope.ProjectId, mr.Title, mr.Description, mr.SourceBranch,
                    mr.CreatedAt == default ? null : mr.CreatedAt, context)).OrderBy(x => x).ToList()
            });
        }

        var allIds = items.SelectMany(i => i.TaskIds).Distinct().ToList();
        if (allIds.Count > 0)
        {
            var tasks = await _backlogRepository.Table
                .Where(x => allIds.Contains(x.Id) && !x.Deleted)
                .Select(x => new ReviewLinkedTaskDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Status = x.Status.Name,
                    TaskType = x.TaskType.Name,
                    StatusGroupId = x.Status.GroupId,
                    TaskTypeGroupId = x.TaskType.GroupId
                })
                .ToDictionaryAsync(x => x.Id);

            foreach (var item in items)
            {
                item.Tasks = item.TaskIds.Where(tasks.ContainsKey).Select(id => tasks[id]).ToList();
                item.TitleIsTaskReference = item.Tasks.Count > 0 && IsOnlyTaskReferences(item.Title);
            }
        }

        return items;
    }

    private static bool IsOnlyTaskReferences(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return true;

        var stripped = System.Text.RegularExpressions.Regex.Replace(title, @"[#\d\s,;:/&+\-_.()\[\]]|\b(and|task|tasks|fix|fixes|wp)\b", string.Empty,
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        return stripped.Length == 0;
    }

    private async Task<List<ReviewLinkedTaskDto>> LoadTasksAsync(GitRepositoryContext context, ReviewMergeRequestDto mr)
    {
        var ids = await _taskCodeService.ResolveTaskIdsAsync(context.Repository.ProjectId, mr.Title, mr.Description, mr.SourceBranch,
            mr.CreatedAt == default ? null : mr.CreatedAt, context);
        if (ids.Count == 0)
            return [];

        return await _backlogRepository.Table
            .Where(x => ids.Contains(x.Id) && !x.Deleted)
            .OrderBy(x => x.Id)
            .Select(x => new ReviewLinkedTaskDto
            {
                Id = x.Id,
                Title = x.Title,
                Status = x.Status.Name,
                TaskType = x.TaskType.Name,
                StatusGroupId = x.Status.GroupId,
                TaskTypeGroupId = x.TaskType.GroupId,
                Description = x.Description
            })
            .ToListAsync();
    }

    private async Task<(ReviewProjectScopeDto? Scope, GitRepositoryContext? Context, string? Token, string? Error)> PrepareAsync(User user, int repositoryId, bool requireMerge)
    {
        var (scope, context) = await ScopeAsync(user, repositoryId);
        if (scope == null || context == null || !scope.CanReview)
            return (null, null, null, "You are not a reviewer for this repository.");
        if (requireMerge && !scope.CanMerge)
            return (null, null, null, "You are not allowed to merge in this project. Ask for 'Can merge' on the project members screen.");

        var (token, error) = await _gitAccountService.RequireTokenAsync(user, context.Connection, "review and merge");
        return error != null ? (null, null, null, error) : (scope, context, token, null);
    }

    private async Task ClearAsync(GitRepositoryContext context, IEnumerable<int>? taskIds = null)
    {
        await ClearQueueCacheAsync(context.RepositoryId);
        foreach (var id in taskIds ?? [])
            await _taskCodeService.ClearCacheAsync(id);
    }

    private static string? BlockedReason(ReviewDetailDto detail)
    {
        var term = detail.Scope.Term;
        if (detail.MergeRequest.State != "opened")
            return $"This {term} is {detail.MergeRequest.State}.";
        if (!detail.Scope.CanMerge)
            return "You are not allowed to merge in this project.";
        if (!detail.Connected)
            return "Connect your Git account to merge.";

        return detail.Status switch
        {
            ReviewMergeStatus.Mergeable => null,
            ReviewMergeStatus.Draft => "It is a draft. The author must mark it as ready first.",
            ReviewMergeStatus.Conflict => "It has merge conflicts. The author must resolve them (or rebase) first.",
            ReviewMergeStatus.NeedsRebase => "The target branch has moved on. Rebase or update the branch first.",
            ReviewMergeStatus.Checking => "The server is still checking whether it can be merged. Refresh in a moment.",
            ReviewMergeStatus.NotChecked => "It had not been checked recently; the server is checking now. Refresh in a moment.",
            ReviewMergeStatus.Blocked => "A repository rule is blocking the merge (for example unresolved discussions, required reviews or failing checks).",
            _ => "It is not reported as ready to merge."
        };
    }

    private static bool Same(string? a, string? b) =>
        !string.IsNullOrWhiteSpace(a) && !string.IsNullOrWhiteSpace(b) && string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);

    private static MergeRequestRefDto ToRef(ReviewMergeRequestDto mr, GitRepositoryContext context) => new()
    {
        Iid = mr.Iid,
        Title = mr.Title,
        WebUrl = mr.WebUrl,
        AuthorUserName = mr.Author?.UserName,
        Sha = mr.Sha,
        ConnectionId = context.Connection.Id,
        RepositoryId = context.RepositoryId,
        RefPrefix = context.Capabilities.RefPrefix
    };

    private static ReviewQueueItemDto Clone(ReviewQueueItemDto s) => new()
    {
        ProjectId = s.ProjectId,
        ProjectName = s.ProjectName,
        RepositoryId = s.RepositoryId,
        RefPrefix = s.RefPrefix,
        Iid = s.Iid,
        Title = s.Title,
        AuthorName = s.AuthorName,
        AuthorUserName = s.AuthorUserName,
        ReviewerUserNames = s.ReviewerUserNames,
        ReviewerNames = s.ReviewerNames,
        AssigneeUserNames = s.AssigneeUserNames,
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt,
        Status = s.Status,
        SourceBranch = s.SourceBranch,
        TargetBranch = s.TargetBranch,
        WebUrl = s.WebUrl,
        NotesCount = s.NotesCount,
        TaskIds = s.TaskIds,
        Tasks = s.Tasks,
        TitleIsTaskReference = s.TitleIsTaskReference,
        Sha = s.Sha
    };

    #endregion
}
