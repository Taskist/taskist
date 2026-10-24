using Microsoft.EntityFrameworkCore;
using Taskist.Core.Caching;
using Taskist.Core.Common;
using Taskist.Core.Domain.Settings;
using Taskist.Core.Domain.WorkItems;
using Taskist.Core.Dtos.Git;
using Taskist.Core.Enums;
using Taskist.Data.Repository;
using Taskist.Service.Git;

namespace Taskist.Service.WorkItems;

public class TaskCodeService : ITaskCodeService
{
    #region Fields

    private const string CacheKey = "wc.taskcode.{0}";
    private const int CacheMinutes = 3;
    private const int MaxMergeRequests = 10;

    protected readonly IRepository<Backlog> _backlogRepository;
    protected readonly IRepository<GitBranchTask> _gitBranchTaskRepository;
    protected readonly IGitRepositoryService _gitRepositoryService;
    protected readonly ICacheManager _cacheManager;
    protected readonly IntegrationSettings _integrationSettings;

    #endregion

    #region Ctor

    public TaskCodeService(IRepository<Backlog> backlogRepository,
        IRepository<GitBranchTask> gitBranchTaskRepository,
        IGitRepositoryService gitRepositoryService,
        ICacheManager cacheManager,
        IntegrationSettings integrationSettings)
    {
        _backlogRepository = backlogRepository;
        _gitBranchTaskRepository = gitBranchTaskRepository;
        _gitRepositoryService = gitRepositoryService;
        _cacheManager = cacheManager;
        _integrationSettings = integrationSettings;
    }

    #endregion

    #region Methods

    public async Task<TaskCodeDto> GetTaskCodeAsync(int backlogId, bool refresh = false)
    {
        if (!await _gitRepositoryService.AnyConfiguredAsync())
            return new TaskCodeDto { State = TaskCodeState.NotConfigured };

        var key = string.Format(CacheKey, backlogId);
        if (refresh)
            await _cacheManager.RemoveAsync(key);

        return await _cacheManager.GetAsync(key, () => LoadAsync(backlogId), CacheMinutes);
    }

    public async Task ClearCacheAsync(int backlogId)
    {
        await _cacheManager.RemoveAsync(string.Format(CacheKey, backlogId));
    }

    public async Task<IList<int>> ResolveTaskIdsForMergeRequestAsync(GitMergeRequestInfo mergeRequest, GitRepositoryContext repository)
    {
        return await ResolveTaskIdsAsync(repository.Repository.ProjectId, mergeRequest.Title, mergeRequest.Description, mergeRequest.SourceBranch,
            mergeRequest.CreatedAt, repository);
    }

    public async Task<IList<int>> ResolveTaskIdsAsync(int taskistProjectId, string? title, string? description, string? sourceBranch,
        DateTime? mergeRequestCreatedAt, GitRepositoryContext? repository)
    {
        var keyPrefix = _integrationSettings.BranchTaskKeyPrefix;
        var candidates = new HashSet<int>(BranchNameBuilder.ExtractMentionedTaskIds(title, keyPrefix));
        foreach (var id in BranchNameBuilder.ExtractMentionedTaskIds(description, keyPrefix))
            candidates.Add(id);

        var linkedIds = new List<int>();
        var isGroupBranch = false;
        if (!string.IsNullOrWhiteSpace(sourceBranch) && repository != null)
        {
            var repositoryId = repository.RepositoryId;
            var refs = new[] { repository.RepoRef, repository.Repository.ExternalRepoId }
                .Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => r!).ToList();

            var links = await _gitBranchTaskRepository.Table
                .Where(x => !x.GitBranch.Deleted
                            && x.GitBranch.Name == sourceBranch
                            && (x.GitBranch.ProjectRepositoryId == repositoryId
                                || (x.GitBranch.ProjectRepositoryId == null && x.GitBranch.ProjectId == taskistProjectId && refs.Contains(x.GitBranch.GitProjectReference))))
                .Select(x => new { x.BacklogId, x.GitBranch.Origin })
                .ToListAsync();
            linkedIds = links.Select(x => x.BacklogId).ToList();
            isGroupBranch = links.Count > 0 && links.All(x => x.Origin == (int)GitBranchOrigin.Created);
        }

        if (!string.IsNullOrWhiteSpace(sourceBranch))
            foreach (var id in BranchNameBuilder.ExtractTaskIds(sourceBranch, keyPrefix))
                candidates.Add(id);

        candidates.ExceptWith(linkedIds);
        if (linkedIds.Count > 0)
        {
            var known = await KnownTaskIdsAsync(taskistProjectId, title, description);
            linkedIds = BranchNameBuilder.TasksForBranchMergeRequest(linkedIds.Distinct().ToList(), known,
                sourceBranch, keyPrefix, title, description,
                isGroupBranch, _integrationSettings.SharedBranchFallbackLimit).ToList();
        }

        var result = new HashSet<int>();

        if (linkedIds.Count > 0)
        {
            var linked = await _backlogRepository.Table
                .Where(x => linkedIds.Contains(x.Id) && !x.Deleted)
                .Select(x => x.Id)
                .ToListAsync();
            result.UnionWith(linked);
        }

        if (candidates.Count > 0)
        {
            var idList = candidates.ToList();
            var tasks = await _backlogRepository.Table
                .Where(x => idList.Contains(x.Id) && !x.Deleted && (taskistProjectId <= 0 || x.ProjectId == taskistProjectId))
                .Select(x => new { x.Id, x.CreatedOn })
                .ToListAsync();

            result.UnionWith(tasks
                .Where(x => mergeRequestCreatedAt == null || mergeRequestCreatedAt.Value >= CreatedFloor(x.CreatedOn))
                .Select(x => x.Id));
        }

        return result.ToList();
    }

    #endregion

    #region Utilities

    private async Task<ISet<int>> KnownTaskIdsAsync(int taskistProjectId, params string?[] texts)
    {
        var numbers = BranchNameBuilder.ExtractTaskReferences(_integrationSettings.BranchTaskKeyPrefix, texts).ToList();
        if (numbers.Count == 0)
            return new HashSet<int>();

        var ids = await _backlogRepository.Table
            .Where(x => numbers.Contains(x.Id) && (taskistProjectId <= 0 || x.ProjectId == taskistProjectId))
            .Select(x => x.Id)
            .ToListAsync();

        return ids.ToHashSet();
    }

    private static DateTime CreatedFloor(DateTime taskCreatedOn)
    {
        return taskCreatedOn.AddDays(-1);
    }

    private async Task<TaskCodeDto> LoadAsync(int backlogId)
    {
        var task = await _backlogRepository.GetByIdAsync(backlogId);
        if (task == null)
            return new TaskCodeDto { State = TaskCodeState.NotLinked };

        var contexts = await _gitRepositoryService.GetProjectContextsAsync(task.ProjectId);
        if (contexts.Count == 0)
            return new TaskCodeDto { State = TaskCodeState.NotLinked };

        var primary = contexts[0];
        var result = new TaskCodeDto
        {
            MultipleRepositories = contexts.Count > 1,
            ShortTerm = primary.Capabilities.ShortTerm,
            Term = primary.Capabilities.Term,
            ProjectWebUrl = primary.Repository.WebUrl?.TrimEnd('/')
        };

        var links = await _gitBranchTaskRepository.Table
            .Where(x => x.BacklogId == backlogId)
            .OrderBy(x => x.GitBranch.Deleted).ThenByDescending(x => x.GitBranch.CreatedOn)
            .Select(x => new
            {
                x.GitBranchId,
                x.GitBranch.Deleted,
                x.GitBranch.Origin,
                x.GitBranch.Name,
                x.GitBranch.BaseBranch,
                x.GitBranch.CreatedOn,
                x.GitBranch.ProjectRepositoryId,
                x.GitBranch.GitProjectReference,
                CreatedBy = x.GitBranch.CreatedBy.FirstName + " " + x.GitBranch.CreatedBy.LastName,
                Shared = x.GitBranch.Tasks.Where(t => t.BacklogId != backlogId).Select(t => t.BacklogId).ToList()
            })
            .ToListAsync();

        var branchContexts = new List<(TaskCodeDto.Branch Branch, GitRepositoryContext Context)>();
        foreach (var link in links)
        {
            var context = contexts.FirstOrDefault(c => c.RepositoryId == link.ProjectRepositoryId)
                          ?? contexts.FirstOrDefault(c => c.Matches(link.GitProjectReference))
                          ?? primary;

            var branch = new TaskCodeDto.Branch
            {
                Id = link.GitBranchId,
                Name = link.Name,
                BaseBranch = link.BaseBranch,
                CreatedBy = link.CreatedBy,
                CreatedOn = link.CreatedOn,
                SharedWithTaskIds = link.Shared.OrderBy(id => id).ToList(),
                Deleted = link.Deleted,
                Origin = link.Origin,
                RepositoryId = context.RepositoryId,
                RepositoryName = context.Name,
                Url = link.Deleted ? null : context.Provider.BranchUrl(context.Repository.WebUrl, link.Name)
            };
            result.Branches.Add(branch);
            branchContexts.Add((branch, context));
        }

        var found = new Dictionary<(int RepositoryId, int Iid), (MergeRequestDetailsDto MergeRequest, GitRepositoryContext Context)>();
        var calls = 0;
        var failures = 0;
        string? lastError = null;

        var branchMergeRequests = new List<(TaskCodeDto.Branch Branch, GitRepositoryContext Context, List<MergeRequestDetailsDto> List)>();
        foreach (var (branch, context) in branchContexts)
        {
            calls++;
            var list = await context.Provider.GetMergeRequestsBySourceBranchAsync(context.RepoRef, branch.Name);
            if (list == null) { failures++; lastError = context.Provider.LastError; continue; }
            branchMergeRequests.Add((branch, context, list));
        }

        var keyPrefix = _integrationSettings.BranchTaskKeyPrefix;
        var known = await KnownTaskIdsAsync(task.ProjectId,
            branchMergeRequests.SelectMany(b => b.List).SelectMany(mr => new[] { mr.Title, mr.Description }).ToArray());

        foreach (var (branch, context, list) in branchMergeRequests)
        {
            var branchTasks = branch.SharedWithTaskIds.Append(backlogId).ToList();
            foreach (var mr in list.Where(mr => BranchNameBuilder.TasksForBranchMergeRequest(branchTasks, known,
                         mr.SourceBranch ?? branch.Name, keyPrefix, mr.Title, mr.Description,
                         branch.Origin == (int)GitBranchOrigin.Created, _integrationSettings.SharedBranchFallbackLimit).Contains(backlogId)))
                found.TryAdd((context.RepositoryId, mr.MrId), (mr, context));
        }

        var floor = CreatedFloor(task.CreatedOn);
        foreach (var context in contexts)
        {
            calls++;
            var searched = await context.Provider.SearchMergeRequestsAsync(context.RepoRef, backlogId.ToString());
            if (searched == null)
            {
                failures++;
                lastError = context.Provider.LastError;
                continue;
            }

            foreach (var mr in searched.Where(mr => (mr.CreatedAt == default || mr.CreatedAt >= floor) && (
                         BranchNameBuilder.ExtractMentionedTaskIds(mr.Title, keyPrefix).Contains(backlogId)
                         || BranchNameBuilder.ExtractMentionedTaskIds(mr.Description, keyPrefix).Contains(backlogId)
                         || BranchNameBuilder.ExtractTaskIds(mr.SourceBranch, keyPrefix).Contains(backlogId))))
                found.TryAdd((context.RepositoryId, mr.MrId), (mr, context));
        }

        if (task.MergeRequestId is > 0)
        {
            var closingContext = contexts.FirstOrDefault(c => c.RepositoryId == task.MergeRequestRepositoryId) ?? primary;
            if (!found.ContainsKey((closingContext.RepositoryId, task.MergeRequestId.Value)))
            {
                calls++;
                var closing = await closingContext.Provider.GetMergeRequestAsync(closingContext.RepoRef, task.MergeRequestId.Value);
                if (closing == null || closing.MrId == 0) { failures++; lastError = closingContext.Provider.LastError; }
                else found.TryAdd((closingContext.RepositoryId, closing.MrId), (closing, closingContext));
            }
        }

        if (failures > 0 && failures == calls)
            return new TaskCodeDto { State = TaskCodeState.Unreachable, Branches = result.Branches, ErrorMessage = lastError, MultipleRepositories = result.MultipleRepositories };

        result.Partial = failures > 0;
        result.ErrorMessage = failures > 0 ? lastError : null;
        result.MergeRequests = found.Values
            .OrderByDescending(x => x.MergeRequest.UpdatedAt)
            .Take(MaxMergeRequests)
            .Select(x => new TaskCodeDto.MergeRequest
            {
                Iid = x.MergeRequest.MrId,
                Title = x.MergeRequest.Title,
                State = x.MergeRequest.State,
                Draft = x.MergeRequest.Draft,
                SourceBranch = x.MergeRequest.SourceBranch,
                TargetBranch = x.MergeRequest.TargetBranch,
                Author = x.MergeRequest.Author?.Name ?? x.MergeRequest.Author?.UserName,
                CreatedAt = x.MergeRequest.CreatedAt,
                UpdatedAt = x.MergeRequest.UpdatedAt,
                Url = x.MergeRequest.Url,
                RepositoryId = x.Context.RepositoryId,
                RepositoryName = x.Context.Name,
                RefPrefix = x.Context.Capabilities.RefPrefix
            })
            .ToList();

        return result;
    }

    #endregion
}
