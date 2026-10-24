using Microsoft.EntityFrameworkCore;
using Taskist.Core.Common;
using Taskist.Core.Domain.WorkItems;
using Taskist.Core.Dtos.Git;
using Taskist.Core.Enums;
using Taskist.Data.Repository;
using Taskist.Service.Git;

namespace Taskist.Service.WorkItems;

public class BranchCleanupService : IBranchCleanupService
{
    #region Fields

    private const int IdleDays = 30;
    private const int MaxPages = 5;
    private const int PageSize = 100;

    protected readonly IGitBranchService _gitBranchService;
    protected readonly IRepository<GitBranch> _gitBranchRepository;
    protected readonly IGitRepositoryService _gitRepositoryService;
    protected readonly IGitAccountService _gitAccountService;
    protected readonly ITaskCodeService _taskCodeService;
    protected readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public BranchCleanupService(IGitBranchService gitBranchService,
        IRepository<GitBranch> gitBranchRepository,
        IGitRepositoryService gitRepositoryService,
        IGitAccountService gitAccountService,
        ITaskCodeService taskCodeService,
        IWorkContext workContext)
    {
        _gitBranchService = gitBranchService;
        _gitBranchRepository = gitBranchRepository;
        _gitRepositoryService = gitRepositoryService;
        _gitAccountService = gitAccountService;
        _taskCodeService = taskCodeService;
        _workContext = workContext;
    }

    #endregion

    #region Methods

    public async Task<bool> CanCleanupAsync(int userId)
    {
        return await _gitRepositoryService.AnyConfiguredAsync() && (await _gitBranchService.GetBranchCaretakerProjectIdsAsync(userId)).Count > 0;
    }

    public async Task<BranchCleanupDto> GetCandidatesAsync(int userId)
    {
        var result = new BranchCleanupDto { IdleDays = IdleDays };
        if (!await _gitRepositoryService.AnyConfiguredAsync())
        {
            result.Error = "No Git connection is configured. An administrator can add one under Configuration → Git connections.";
            return result;
        }

        var projectIds = await _gitBranchService.GetBranchCaretakerProjectIdsAsync(userId);
        if (projectIds.Count == 0)
            return result;

        var branches = await _gitBranchRepository.Table
            .Where(x => !x.Deleted && projectIds.Contains(x.ProjectId))
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.ProjectId,
                x.ProjectRepositoryId,
                x.GitProjectReference,
                ProjectName = x.Project.Name,
                CreatedBy = x.CreatedBy.FirstName + " " + x.CreatedBy.LastName,
                Tasks = x.Tasks.Select(t => new
                {
                    t.BacklogId,
                    Closed = t.Backlog.Deleted
                             || t.Backlog.Status.GroupId == (int)StatusGroup.Resolved
                             || t.Backlog.Status.GroupId == (int)StatusGroup.Archived
                }).ToList()
            })
            .ToListAsync();

        var user = await _workContext.GetCurrentUserAsync();
        var idleBefore = DateTime.UtcNow.AddDays(-IdleDays);
        var contexts = new Dictionary<int, IList<GitRepositoryContext>>();
        var connectedAll = true;

        foreach (var projectId in branches.Select(b => b.ProjectId).Distinct())
            contexts[projectId] = await _gitRepositoryService.GetProjectContextsAsync(projectId);

        var grouped = branches
            .Select(b => new
            {
                Branch = b,
                Context = contexts[b.ProjectId].FirstOrDefault(c => c.RepositoryId == b.ProjectRepositoryId)
                          ?? contexts[b.ProjectId].FirstOrDefault(c => c.Matches(b.GitProjectReference))
            })
            .Where(x => x.Context != null)
            .GroupBy(x => x.Context!.RepositoryId);

        foreach (var group in grouped)
        {
            var context = group.First().Context!;
            if (user == null || string.IsNullOrWhiteSpace(await _gitAccountService.GetTokenAsync(user, context.Connection)))
                connectedAll = false;

            var (remote, complete) = await LoadRemoteBranchesAsync(context);
            if (remote == null)
            {
                result.FailedProjects.Add(group.First().Branch.ProjectName);
                result.Error = context.Provider.LastError;
                continue;
            }

            foreach (var branch in group.Select(x => x.Branch))
            {
                remote.TryGetValue(branch.Name, out var gl);

                BranchCleanupReason? reason;
                if (gl == null)
                    reason = complete ? BranchCleanupReason.Gone : null;
                else if (gl.Protected || gl.Default)
                    reason = null;
                else if (gl.Merged)
                    reason = BranchCleanupReason.Merged;
                else if (branch.Tasks.Count > 0 && branch.Tasks.All(t => t.Closed))
                    reason = BranchCleanupReason.TasksClosed;
                else if (gl.Commit?.CommittedDate is { } committed && committed.ToUniversalTime() < idleBefore)
                    reason = BranchCleanupReason.Idle;
                else
                    reason = null;

                if (reason == null)
                    continue;

                result.Items.Add(new BranchCleanupItemDto
                {
                    BranchId = branch.Id,
                    Name = branch.Name,
                    ProjectName = contexts[branch.ProjectId].Count > 1 ? $"{branch.ProjectName} / {context.Name}" : branch.ProjectName,
                    Reason = reason.Value,
                    LastCommitOn = gl?.Commit?.CommittedDate,
                    CreatedBy = branch.CreatedBy,
                    TaskIds = branch.Tasks.Select(t => t.BacklogId).OrderBy(t => t).ToList(),
                    WebUrl = gl?.Url ?? context.Provider.BranchUrl(context.Repository.WebUrl, branch.Name)
                });
            }
        }

        result.Connected = connectedAll;
        result.Items = result.Items
            .OrderBy(x => x.Reason)
            .ThenBy(x => x.LastCommitOn ?? DateTime.MinValue)
            .ThenBy(x => x.Name)
            .ToList();

        return result;
    }

    public async Task<BranchCleanupResult> DeleteAsync(int userId, IList<int> branchIds)
    {
        var result = new BranchCleanupResult();
        if (branchIds == null || branchIds.Count == 0)
            return result;

        var user = await _workContext.GetCurrentUserAsync();
        var projectIds = await _gitBranchService.GetBranchCaretakerProjectIdsAsync(userId);
        var ids = branchIds.Distinct().ToList();
        var branches = await _gitBranchRepository.Table
            .Where(x => ids.Contains(x.Id) && !x.Deleted && projectIds.Contains(x.ProjectId))
            .Include(x => x.Tasks)
            .ToListAsync();

        foreach (var branch in branches)
        {
            var context = await _gitRepositoryService.GetBranchContextAsync(branch);
            if (context == null)
            {
                result.Failures.Add($"{branch.Name}: the repository is no longer linked to the project.");
                continue;
            }

            var (token, error) = await _gitAccountService.RequireTokenAsync(user, context.Connection, "delete branches");
            if (error != null)
            {
                result.NeedsConnection = true;
                result.Failures.Add(error);
                continue;
            }

            var response = await context.Provider.DeleteBranchAsync(context.RepoRef, branch.Name, token!);
            if (!response.Success && response.StatusCode != 404 && response.StatusCode != 422)
            {
                result.Failures.Add($"{branch.Name}: {response.Error}");
                continue;
            }

            if (response.Success)
                result.Deleted++;
            else
                result.AlreadyGone++;

            branch.Deleted = true;
            await _gitBranchRepository.UpdateAsync(branch);

            foreach (var link in branch.Tasks)
                await _taskCodeService.ClearCacheAsync(link.BacklogId);
        }

        result.Failures = result.Failures.Distinct().ToList();
        return result;
    }

    #endregion

    #region Utilities

    private static async Task<(Dictionary<string, BranchDto>? Branches, bool Complete)> LoadRemoteBranchesAsync(GitRepositoryContext context)
    {
        var map = new Dictionary<string, BranchDto>(StringComparer.Ordinal);
        for (var page = 1; page <= MaxPages; page++)
        {
            var list = await context.Provider.GetBranchesAsync(context.RepoRef, page);
            if (list == null)
                return page == 1 ? (null, false) : (map, false);

            foreach (var branch in list.Where(b => !string.IsNullOrEmpty(b.Name)))
                map.TryAdd(branch.Name, branch);

            if (list.Count < PageSize)
                return (map, true);
        }

        return (map, false);
    }

    #endregion
}
