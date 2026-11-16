using Microsoft.EntityFrameworkCore;
using Taskist.Core.Caching;
using Taskist.Core.Common;
using Taskist.Core.Domain.Settings;
using Taskist.Core.Domain.Users;
using Taskist.Core.Domain.WorkItems;
using Taskist.Core.Dtos.Git;
using Taskist.Data.Repository;
using Taskist.Service.Git;

namespace Taskist.Service.WorkItems;

public class MyCodeService : IMyCodeService
{
    #region Fields

    private const string CacheKey = "wc.mycode.{0}";
    private const int CacheMinutes = 1;
    private const int MaxItems = 8;
    private const int BranchWindowDays = 60;

    protected readonly IGitRepositoryService _gitRepositoryService;
    protected readonly IGitAccountService _gitAccountService;
    protected readonly IRepository<GitBranch> _gitBranchRepository;
    protected readonly IRepository<Backlog> _backlogRepository;
    protected readonly ICacheManager _cacheManager;
    protected readonly IntegrationSettings _integrationSettings;

    #endregion

    #region Ctor

    public MyCodeService(IGitRepositoryService gitRepositoryService,
        IGitAccountService gitAccountService,
        IRepository<GitBranch> gitBranchRepository,
        IRepository<Backlog> backlogRepository,
        ICacheManager cacheManager,
        IntegrationSettings integrationSettings)
    {
        _gitRepositoryService = gitRepositoryService;
        _gitAccountService = gitAccountService;
        _gitBranchRepository = gitBranchRepository;
        _backlogRepository = backlogRepository;
        _cacheManager = cacheManager;
        _integrationSettings = integrationSettings;
    }

    #endregion

    #region Methods

    public async Task<MyCodeDto> GetAsync(User user, bool refresh = false)
    {
        if (!await _gitRepositoryService.AnyConfiguredAsync())
            return new MyCodeDto { State = MyCodeState.NotConfigured };

        var connectedIds = await _gitAccountService.GetConnectedConnectionIdsAsync(user.Id);
        var connections = (await _gitRepositoryService.GetConnectionsAsync()).Where(c => connectedIds.Contains(c.Id) || c.IsDefault).ToList();
        if (connections.Count == 0)
            return new MyCodeDto { State = MyCodeState.NotConnected };

        var key = string.Format(CacheKey, user.Id);
        if (refresh)
            await _cacheManager.RemoveAsync(key);

        var result = await _cacheManager.GetAsync(key, () => LoadAsync(user, connections), CacheMinutes);
        if (result.State != MyCodeState.Ok)
            await _cacheManager.RemoveAsync(key);

        return result;
    }

    public async Task ClearCacheAsync(int userId)
    {
        await _cacheManager.RemoveAsync(string.Format(CacheKey, userId));
    }

    #endregion

    #region Utilities

    private async Task<MyCodeDto> LoadAsync(User user, IList<Taskist.Core.Domain.Git.GitConnection> connections)
    {
        var mine = new List<(ReviewMergeRequestDto MergeRequest, GitProviderCapabilities Capabilities)>();
        var reviewing = new List<(ReviewMergeRequestDto MergeRequest, GitProviderCapabilities Capabilities)>();
        var reached = 0;
        string? lastError = null;

        foreach (var connection in connections)
        {
            var token = await _gitAccountService.GetTokenAsync(user, connection);
            if (string.IsNullOrWhiteSpace(token))
                continue;

            var provider = _gitRepositoryService.ProviderFor(connection);
            var authored = await provider.GetUserMergeRequestsAsync(new GitUserMergeRequestQuery { Limit = 100 }, token);
            if (authored == null)
            {
                lastError = provider.LastError;
                continue;
            }

            reached++;
            mine.AddRange(authored.Select(mr => (mr, provider.Capabilities)));

            var userName = await _gitAccountService.GetUserNameAsync(user, connection);
            if (string.IsNullOrWhiteSpace(userName))
                continue;

            var requested = await provider.GetUserMergeRequestsAsync(new GitUserMergeRequestQuery { ReviewRequested = true, OpenOnly = true, UserName = userName }, token) ?? [];
            reviewing.AddRange(requested
                .Where(x => !string.Equals(x.Author?.UserName, userName, StringComparison.OrdinalIgnoreCase))
                .Select(mr => (mr, provider.Capabilities)));
        }

        if (reached == 0)
            return lastError == null
                ? new MyCodeDto { State = MyCodeState.NotConnected }
                : new MyCodeDto { State = MyCodeState.Unreachable, Error = lastError };

        var opened = mine.Where(x => string.Equals(x.MergeRequest.State, "opened", StringComparison.OrdinalIgnoreCase)).ToList();
        var result = new MyCodeDto
        {
            Authored = opened.OrderByDescending(x => x.MergeRequest.UpdatedAt).Take(MaxItems).Select(x => ToItem(x.MergeRequest, x.Capabilities)).ToList(),
            Reviewing = reviewing.OrderBy(x => x.MergeRequest.UpdatedAt).Take(MaxItems).Select(x => ToItem(x.MergeRequest, x.Capabilities)).ToList()
        };

        await KeepKnownTasksAsync(result.Authored.Concat(result.Reviewing).ToList());

        var since = DateTime.UtcNow.AddDays(-BranchWindowDays);
        var branches = await _gitBranchRepository.Table
            .Where(x => x.CreatedById == user.Id && !x.Deleted && x.CreatedOn >= since
                        && x.Project.Active && !x.Project.Deleted
                        && x.Tasks.Any(t => !t.Backlog.Deleted))
            .OrderByDescending(x => x.CreatedOn)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.GitProjectReference,
                ProjectName = x.Project.Name,
                x.CreatedOn,
                TaskIds = x.Tasks.Where(t => !t.Backlog.Deleted).OrderBy(t => t.BacklogId).Select(t => t.BacklogId).ToList()
            })
            .Take(40)
            .ToListAsync();

        result.Branches = branches
            .Where(b => !mine.Any(mr => string.Equals(mr.MergeRequest.SourceBranch, b.Name, StringComparison.Ordinal) && SameProject(mr.MergeRequest, b.GitProjectReference)))
            .Take(MaxItems)
            .Select(b => new MyCodeBranchDto
            {
                BranchId = b.Id,
                Name = b.Name,
                ProjectName = b.ProjectName,
                TaskId = b.TaskIds.First(),
                TaskCount = b.TaskIds.Count,
                CreatedOn = b.CreatedOn
            })
            .ToList();

        return result;
    }

    private MyCodeMergeRequestDto ToItem(ReviewMergeRequestDto mr, GitProviderCapabilities capabilities)
    {
        var keyPrefix = _integrationSettings.BranchTaskKeyPrefix;
        var taskIds = new HashSet<int>(BranchNameBuilder.ExtractMentionedTaskIds(mr.Title, keyPrefix));
        foreach (var id in BranchNameBuilder.ExtractTaskIds(mr.SourceBranch, keyPrefix))
            taskIds.Add(id);

        return new MyCodeMergeRequestDto
        {
            Iid = mr.Iid,
            RefPrefix = capabilities.RefPrefix,
            Title = mr.Title,
            ProjectPath = ProjectPath(mr.WebUrl),
            WebUrl = mr.WebUrl,
            SourceBranch = mr.SourceBranch,
            TargetBranch = mr.TargetBranch,
            Status = ReviewMergeStatus.From(mr),
            AuthorName = mr.Author?.Name ?? mr.Author?.UserName,
            ReviewerNames = mr.Reviewers.Select(r => r.Name ?? r.UserName ?? "?").ToList(),
            NotesCount = mr.UserNotesCount,
            UpdatedAt = mr.UpdatedAt,
            TaskIds = taskIds.OrderBy(x => x).ToList()
        };
    }

    private async Task KeepKnownTasksAsync(IList<MyCodeMergeRequestDto> items)
    {
        var ids = items.SelectMany(x => x.TaskIds).Distinct().ToList();
        if (ids.Count == 0)
            return;

        var known = (await _backlogRepository.Table
            .Where(x => ids.Contains(x.Id) && !x.Deleted)
            .Select(x => x.Id)
            .ToListAsync()).ToHashSet();

        foreach (var item in items)
            item.TaskIds = item.TaskIds.Where(known.Contains).ToList();
    }

    private static bool SameProject(ReviewMergeRequestDto mr, string? projectReference)
    {
        var reference = projectReference?.Trim().Trim('/');
        if (string.IsNullOrEmpty(reference))
            return false;

        if (reference == mr.ProjectId.ToString())
            return true;

        var path = ProjectPath(mr.WebUrl);
        return path != null && (path.Equals(reference, StringComparison.OrdinalIgnoreCase)
                                || path.EndsWith("/" + reference, StringComparison.OrdinalIgnoreCase));
    }

    private static string? ProjectPath(string? webUrl)
    {
        if (string.IsNullOrWhiteSpace(webUrl) || !Uri.TryCreate(webUrl, UriKind.Absolute, out var uri))
            return null;

        var path = uri.AbsolutePath;
        var marker = path.IndexOf("/-/", StringComparison.Ordinal);
        if (marker < 0)
            marker = path.IndexOf("/pull/", StringComparison.Ordinal);
        return (marker >= 0 ? path[..marker] : path).Trim('/');
    }

    #endregion
}
