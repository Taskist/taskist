using Microsoft.EntityFrameworkCore;
using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;
using Taskist.Core.Domain.Settings;
using Taskist.Core.Domain.Users;
using Taskist.Core.Domain.WorkItems;
using Taskist.Core.Dtos.Git;
using Taskist.Core.Enums;
using Taskist.Core.Extensions;
using Taskist.Data.Repository;
using Taskist.Service.Git;
using Taskist.Service.Users;

namespace Taskist.Service.WorkItems;

public class GitBranchService : IGitBranchService
{
    #region Fields

    protected readonly IRepository<Backlog> _backlogRepository;
    protected readonly IRepository<GitBranch> _gitBranchRepository;
    protected readonly IRepository<GitBranchTask> _gitBranchTaskRepository;
    protected readonly IRepository<BacklogComment> _backlogCommentRepository;
    protected readonly IRepository<Project> _projectRepository;
    protected readonly IRepository<UserProjectMap> _userProjectMapRepository;
    protected readonly IGitRepositoryService _gitRepositoryService;
    protected readonly IGitAccountService _gitAccountService;
    protected readonly IUserService _userService;
    protected readonly ITaskCodeService _taskCodeService;
    protected readonly IWorkContext _workContext;
    protected readonly IntegrationSettings _integrationSettings;
    protected readonly IBacklogService _backlogItemService;
    protected readonly Taskist.Service.Masters.IStatusService _statusService;

    #endregion

    #region Ctor

    public GitBranchService(IRepository<Backlog> backlogRepository,
        IRepository<GitBranch> gitBranchRepository,
        IRepository<GitBranchTask> gitBranchTaskRepository,
        IRepository<BacklogComment> backlogCommentRepository,
        IRepository<Project> projectRepository,
        IRepository<UserProjectMap> userProjectMapRepository,
        IGitRepositoryService gitRepositoryService,
        IGitAccountService gitAccountService,
        IUserService userService,
        ITaskCodeService taskCodeService,
        IWorkContext workContext,
        IntegrationSettings integrationSettings,
        IBacklogService backlogItemService,
        Taskist.Service.Masters.IStatusService statusService)
    {
        _backlogRepository = backlogRepository;
        _gitBranchRepository = gitBranchRepository;
        _gitBranchTaskRepository = gitBranchTaskRepository;
        _backlogCommentRepository = backlogCommentRepository;
        _projectRepository = projectRepository;
        _userProjectMapRepository = userProjectMapRepository;
        _gitRepositoryService = gitRepositoryService;
        _gitAccountService = gitAccountService;
        _userService = userService;
        _taskCodeService = taskCodeService;
        _workContext = workContext;
        _integrationSettings = integrationSettings;
        _backlogItemService = backlogItemService;
        _statusService = statusService;
    }

    #endregion

    #region Methods

    public bool CreationEnabled => _integrationSettings.AllowBranchCreation;

    public async Task<bool> CanManageBranchAsync(int backlogId)
    {
        var task = await _backlogRepository.GetByIdAsync(backlogId);
        var user = await _workContext.GetCurrentUserAsync();
        return task != null && await CheckTaskAsync(task, user.Id) == null;
    }

    public async Task<BranchContextDto> GetContextAsync(IList<int> backlogIds, int? repositoryId = null)
    {
        var context = new BranchContextDto { CanCreate = _integrationSettings.AllowBranchCreation };
        if (!await _gitRepositoryService.AnyConfiguredAsync())
        {
            context.Error = "No Git connection is configured. An administrator can add one under Configuration → Git connections.";
            return context;
        }

        var (eligible, skipped, repository, repositories) = await EvaluateAsync(backlogIds, repositoryId);
        context.Skipped = skipped;
        context.EligibleTaskIds = eligible.Select(x => x.Id).ToList();
        context.Repositories = repositories.Select(r => new BranchContextDto.RepositoryOption { Id = r.RepositoryId, Name = r.Name }).ToList();

        if (eligible.Count == 0 || repository == null)
        {
            context.Error = skipped.Count == 1 ? skipped[0].Reason : "None of the selected tasks can be linked to a branch.";
            return context;
        }

        context.RepositoryId = repository.RepositoryId;

        var existingWork = await ExistingWorkAsync(eligible);
        context.Existing = existingWork.Select(x => new BranchContextDto.SkippedTask { TaskId = x.Key, Reason = x.Value }).OrderBy(x => x.TaskId).ToList();
        var fresh = eligible.Where(t => !existingWork.ContainsKey(t.Id)).ToList();
        if (fresh.Count > 0)
        {
            eligible = fresh;
            context.EligibleTaskIds = fresh.Select(x => x.Id).ToList();
        }
        else
        {
            context.EligibleTaskIds = [];
        }

        var provider = repository.Provider;
        var project = await provider.GetRepositoryAsync(repository.RepoRef);
        if (project == null)
        {
            context.Error = provider.LastError ?? "Could not load the repository.";
            return context;
        }

        context.DefaultBranch = project.DefaultBranch;
        var branches = await provider.SearchBranchesAsync(repository.RepoRef, null);
        if (branches == null)
        {
            context.Error = provider.LastError ?? "Could not load branches.";
            return context;
        }

        context.BaseBranches = branches.OrderByDescending(b => b.Default).ThenBy(b => b.Name).Select(b => b.Name).ToList();
        if (!string.IsNullOrEmpty(project.DefaultBranch) && !context.BaseBranches.Contains(project.DefaultBranch))
            context.BaseBranches.Insert(0, project.DefaultBranch);

        if (context.CanCreate)
        {
            var (_, connectionError) = await PersonalTokenAsync(repository, "create branches");
            context.NeedsConnection = connectionError != null;
            context.ConnectionMessage = connectionError;
        }

        context.Prefix = BranchNameBuilder.PrefixFor(eligible.Select(x => x.TaskType?.GroupId ?? 0));
        context.SuggestedName = BranchNameBuilder.Build(context.Prefix, eligible.Select(x => x.Id).ToList(),
            eligible.OrderBy(x => x.Id).First().Title, _integrationSettings.BranchTaskKeyPrefix);

        return context;
    }

    public async Task<BranchActionResult> CreateBranchAsync(IList<int> backlogIds, string branchName, string baseBranch, bool includeExisting = false, int? repositoryId = null)
    {
        if (!_integrationSettings.AllowBranchCreation)
            return BranchActionResult.Fail("Branch creation from Taskist is turned off. An administrator can enable it under Configuration → Integration.");

        var name = branchName?.Trim() ?? string.Empty;
        var nameError = BranchNameBuilder.ValidateName(name);
        if (nameError != null)
            return BranchActionResult.Fail(nameError);
        if (string.IsNullOrWhiteSpace(baseBranch))
            return BranchActionResult.Fail("Choose the branch to create it from.");

        var (eligible, skipped, repository, _) = await EvaluateAsync(backlogIds, repositoryId);
        if (eligible.Count == 0 || repository == null)
            return BranchActionResult.Fail(skipped.FirstOrDefault()?.Reason ?? "None of the selected tasks can be linked to a branch.");

        if (!includeExisting)
        {
            var existing = await ExistingWorkAsync(eligible);
            eligible = eligible.Where(t => !existing.ContainsKey(t.Id)).ToList();
            if (eligible.Count == 0)
                return BranchActionResult.Fail("All selected tasks already have a branch or are already in review. Tick \"Create a new branch for these tasks anyway\" if you really need another branch.");
        }

        var (token, connectionError) = await PersonalTokenAsync(repository, "create branches");
        if (connectionError != null)
            return BranchActionResult.Connect(connectionError);

        var created = await repository.Provider.CreateBranchAsync(repository.RepoRef, name, baseBranch.Trim(), token!);
        if (!created.Success)
            return created.StatusCode == 401
                ? BranchActionResult.Connect(created.Error ?? "Your token was rejected.")
                : BranchActionResult.Fail(created.Error ?? "The branch was not created.");

        var user = await _workContext.GetCurrentUserAsync();
        var branch = await FindBranchAsync(repository, name) ?? new GitBranch
        {
            ProjectId = eligible[0].ProjectId,
            ProjectRepositoryId = repository.RepositoryId,
            GitProjectReference = repository.RepoRef,
            Name = name,
            BaseBranch = baseBranch.Trim(),
            Origin = (int)GitBranchOrigin.Created,
            CreatedById = user.Id,
            CreatedOn = DateTime.UtcNow
        };

        if (branch.Id == 0)
            await _gitBranchRepository.InsertAsync(branch);

        var linked = await LinkAsync(branch, eligible, user.Id,
            ids => $"Branch {name} created from {baseBranch.Trim()} by {user.Name}{RepositoryText(repository)}{SharedText(ids)}");

        return new BranchActionResult { Success = true, BranchName = name, LinkedTaskIds = linked };
    }

    public async Task<BranchActionResult> LinkBranchAsync(IList<int> backlogIds, string branchName, int? repositoryId = null)
    {
        var name = branchName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
            return BranchActionResult.Fail("Choose a branch to link.");

        var (eligible, skipped, repository, _) = await EvaluateAsync(backlogIds, repositoryId);
        if (eligible.Count == 0 || repository == null)
            return BranchActionResult.Fail(skipped.FirstOrDefault()?.Reason ?? "None of the selected tasks can be linked to a branch.");

        var matches = await repository.Provider.SearchBranchesAsync(repository.RepoRef, name);
        if (matches == null)
            return BranchActionResult.Fail(repository.Provider.LastError ?? "Could not check the branch.");
        if (!matches.Any(b => b.Name == name))
            return BranchActionResult.Fail($"Branch '{name}' was not found in {repository.Name}.");

        var user = await _workContext.GetCurrentUserAsync();
        var branch = await FindBranchAsync(repository, name);
        if (branch == null)
        {
            branch = new GitBranch
            {
                ProjectId = eligible[0].ProjectId,
                ProjectRepositoryId = repository.RepositoryId,
                GitProjectReference = repository.RepoRef,
                Name = name,
                Origin = (int)GitBranchOrigin.Linked,
                CreatedById = user.Id,
                CreatedOn = DateTime.UtcNow
            };
            await _gitBranchRepository.InsertAsync(branch);
        }

        var linked = await LinkAsync(branch, eligible, user.Id,
            ids => $"Linked to branch {name}{RepositoryText(repository)} by {user.Name}{SharedText(ids)}");

        if (linked.Count == 0)
            return BranchActionResult.Fail("The selected task(s) are already linked to this branch.");

        return new BranchActionResult { Success = true, BranchName = name, LinkedTaskIds = linked };
    }

    public async Task LinkClosingMergeRequestBranchAsync(int backlogId, int mergeRequestIid, int? repositoryId = null)
    {
        if (mergeRequestIid <= 0)
            return;

        var task = await _backlogRepository.GetByIdAsync(backlogId);
        if (task == null)
            return;

        var repository = repositoryId > 0
            ? await _gitRepositoryService.GetContextAsync(repositoryId.Value)
            : await _gitRepositoryService.GetClosingContextAsync(task);
        if (repository == null || repository.Repository.ProjectId != task.ProjectId)
            return;

        var mr = await repository.Provider.GetMergeRequestAsync(repository.RepoRef, mergeRequestIid);
        var name = mr?.SourceBranch?.Trim();
        if (mr == null || mr.MrId == 0 || string.IsNullOrWhiteSpace(name))
            return;

        var matches = await repository.Provider.SearchBranchesAsync(repository.RepoRef, name);
        if (matches == null)
            return;

        var exists = matches.Any(b => b.Name == name);
        var user = await _workContext.GetCurrentUserAsync();
        if (user == null)
            return;

        var repoId = repository.RepositoryId;
        var repoRef = repository.RepoRef;
        var branch = await _gitBranchRepository.Table
            .Where(x => (x.ProjectRepositoryId == repoId || (x.ProjectRepositoryId == null && x.GitProjectReference == repoRef))
                        && x.Name == name && x.Deleted == !exists)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync();

        if (branch == null)
        {
            branch = new GitBranch
            {
                ProjectId = task.ProjectId,
                ProjectRepositoryId = repoId,
                GitProjectReference = repoRef,
                Name = name,
                BaseBranch = mr.TargetBranch,
                CreatedById = user.Id,
                CreatedOn = mr.CreatedAt == default ? DateTime.UtcNow : mr.CreatedAt,
                Deleted = !exists,
                Origin = (int)GitBranchOrigin.ClosingMergeRequest
            };
            await _gitBranchRepository.InsertAsync(branch);
        }

        await LinkAsync(branch, [task], user.Id,
            ids => $"Linked to branch {name} from closing {repository.Capabilities.Label(mergeRequestIid)}{(exists ? string.Empty : " (already deleted)")}");
    }

    public async Task<MergeRequestContextDto> GetMergeRequestContextAsync(int backlogId, int gitBranchId)
    {
        var context = new MergeRequestContextDto();
        var (task, branch, repository, linkedTasks, error) = await PrepareMergeRequestAsync(backlogId, gitBranchId);
        if (error != null)
        {
            context.Error = error;
            return context;
        }

        var provider = repository!.Provider;
        context.ShortTerm = provider.Capabilities.ShortTerm;
        context.Term = provider.Capabilities.Term;
        context.RefPrefix = provider.Capabilities.RefPrefix;
        context.RepositoryName = repository.Name;

        var (_, connectionError) = await PersonalTokenAsync(repository, $"create {provider.Capabilities.Term}s");
        context.NeedsConnection = connectionError != null;
        context.ConnectionMessage = connectionError;
        context.SourceBranch = branch!.Name;
        context.TaskIds = linkedTasks.Select(t => t.Id).ToList();
        context.Title = MergeRequestTitle(linkedTasks);
        context.Description = MergeRequestDescription(linkedTasks);

        var existing = await provider.GetMergeRequestsBySourceBranchAsync(repository.RepoRef, branch.Name);
        var open = existing?.FirstOrDefault(m => string.Equals(m.State, "opened", StringComparison.OrdinalIgnoreCase));
        if (open != null)
        {
            context.ExistingIid = open.MrId;
            context.ExistingUrl = open.Url;
        }

        var project = await provider.GetRepositoryAsync(repository.RepoRef);
        var branches = await provider.SearchBranchesAsync(repository.RepoRef, null);
        if (project == null || branches == null)
        {
            context.Error = provider.LastError ?? "Could not load the repository.";
            return context;
        }

        context.TargetBranches = branches.Where(b => b.Name != branch.Name)
            .OrderByDescending(b => b.Default).ThenBy(b => b.Name).Select(b => b.Name).ToList();
        if (!string.IsNullOrEmpty(project.DefaultBranch) && !context.TargetBranches.Contains(project.DefaultBranch))
            context.TargetBranches.Insert(0, project.DefaultBranch);
        context.DefaultTarget = !string.IsNullOrWhiteSpace(branch.BaseBranch) && context.TargetBranches.Contains(branch.BaseBranch)
            ? branch.BaseBranch
            : project.DefaultBranch;

        var user = await _workContext.GetCurrentUserAsync();
        context.ReviewerName = (await ReviewerForAsync(task!.Project, user, repository))?.User.Name;

        if (_integrationSettings.MoveTasksOnMergeRequestOpen)
        {
            var opened = _integrationSettings.OpenedStatusId > 0 ? await _statusService.GetByIdAsync(_integrationSettings.OpenedStatusId.Value) : null;
            context.OpenedStatusName = opened?.Name ?? MergeStatusDefaults.Opened;
        }

        return context;
    }

    public async Task<MergeRequestCreatedResult> CreateMergeRequestAsync(int backlogId, int gitBranchId, string title, string targetBranch,
        string? description, bool draft, bool removeSourceBranch)
    {
        title = title?.Trim() ?? string.Empty;
        if (title.Length == 0)
            return MergeRequestCreatedResult.Fail("Enter a title.");
        if (string.IsNullOrWhiteSpace(targetBranch))
            return MergeRequestCreatedResult.Fail("Choose the branch to merge into.");

        var (task, branch, repository, linkedTasks, error) = await PrepareMergeRequestAsync(backlogId, gitBranchId);
        if (error != null)
            return MergeRequestCreatedResult.Fail(error);
        if (string.Equals(branch!.Name, targetBranch.Trim(), StringComparison.Ordinal))
            return MergeRequestCreatedResult.Fail("The source and target branch must be different.");

        var provider = repository!.Provider;
        var (token, connectionError) = await PersonalTokenAsync(repository, $"create {provider.Capabilities.Term}s");
        if (connectionError != null)
            return MergeRequestCreatedResult.Fail(connectionError, needsConnection: true);

        var user = await _workContext.GetCurrentUserAsync();
        var reviewer = await ReviewerForAsync(task!.Project, user, repository);

        var created = await provider.CreateMergeRequestAsync(repository.RepoRef, branch.Name, targetBranch.Trim(), title,
            description?.Trim(), reviewer?.UserName, removeSourceBranch, draft, token!);
        if (!created.Success || created.Data == null)
            return MergeRequestCreatedResult.Fail(created.Error ?? $"The {provider.Capabilities.Term} was not created.", created.StatusCode == 401);

        var mr = created.Data;
        var label = provider.Capabilities.Label(mr.MrId);
        await _backlogCommentRepository.InsertAsync(linkedTasks
            .Select(t => Comment(t.Id, user!.Id, $"{label} created from Taskist by {user.Name}: {branch.Name} → {targetBranch.Trim()}{(draft ? " (draft)" : string.Empty)}"))
            .ToList());

        var moved = 0;
        if (!draft)
        {
            var taskIds = await _taskCodeService.ResolveTaskIdsAsync(task.ProjectId, title, description, branch.Name, DateTime.UtcNow, repository);
            moved = await _backlogItemService.MoveTasksForOpenedMergeRequestAsync(taskIds.Union(linkedTasks.Select(t => t.Id)).ToList(), mr.MrId,
                user, null, repository.RepositoryId, label);
        }

        foreach (var t in linkedTasks)
            await _taskCodeService.ClearCacheAsync(t.Id);

        return new MergeRequestCreatedResult { Success = true, Iid = mr.MrId, Url = mr.Url, MovedTasks = moved, RefPrefix = provider.Capabilities.RefPrefix };
    }

    public async Task<BranchActionResult> UnlinkBranchAsync(int backlogId, int gitBranchId)
    {
        var task = await _backlogRepository.GetByIdAsync(backlogId);
        var user = await _workContext.GetCurrentUserAsync();
        if (task == null)
            return BranchActionResult.Fail("Task not found.");

        var link = await _gitBranchTaskRepository.Table
            .FirstOrDefaultAsync(x => x.BacklogId == backlogId && x.GitBranchId == gitBranchId);
        if (link == null)
            return BranchActionResult.Fail("This branch is not linked to the task.");

        if (link.LinkedById != user.Id && await CheckTaskAsync(task, user.Id, requireOpen: false) != null)
            return BranchActionResult.Fail("You are not allowed to change branches on this task.");

        var name = link.GitBranch?.Name;
        await _gitBranchTaskRepository.DeleteAsync(link);
        await _backlogCommentRepository.InsertAsync(Comment(backlogId, user.Id, $"Unlinked from branch {name} by {user.Name}"));
        await _taskCodeService.ClearCacheAsync(backlogId);

        return new BranchActionResult { Success = true, BranchName = name };
    }

    public async Task<(List<string>? Branches, string? Error)> SearchBranchesAsync(int backlogId, string? term, int? repositoryId = null)
    {
        var task = await _backlogRepository.GetByIdAsync(backlogId);
        if (task == null)
            return (null, "Task not found.");

        var repository = await ResolveRepositoryAsync(task.ProjectId, repositoryId);
        if (repository == null)
            return (null, "This project is not linked to a repository.");

        var branches = await repository.Provider.SearchBranchesAsync(repository.RepoRef, term);
        return branches == null
            ? (null, repository.Provider.LastError ?? "Could not load branches.")
            : (branches.Select(b => b.Name).OrderBy(n => n).ToList(), null);
    }

    public async Task HandlePushAsync(GitPushInfo push, GitRepositoryContext repository, User? pusher)
    {
        var branchName = push?.BranchName;
        if (string.IsNullOrWhiteSpace(branchName))
            return;

        var pusherName = pusher?.Name ?? (string.IsNullOrWhiteSpace(push!.PusherUserName) ? "someone" : "@" + push.PusherUserName);
        var repoId = repository.RepositoryId;
        var refs = new[] { repository.RepoRef, repository.Repository.ExternalRepoId }.Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => r!).ToList();
        var projectId = repository.Repository.ProjectId;

        var existing = await _gitBranchRepository.Table
            .Where(x => !x.Deleted && x.Name == branchName
                        && (x.ProjectRepositoryId == repoId || (x.ProjectRepositoryId == null && x.ProjectId == projectId && refs.Contains(x.GitProjectReference))))
            .ToListAsync();

        if (push!.IsDeletedBranch)
        {
            foreach (var branch in existing)
            {
                branch.Deleted = true;
                var taskIds = await _gitBranchTaskRepository.Table.Where(x => x.GitBranchId == branch.Id).Select(x => x.BacklogId).ToListAsync();
                if (taskIds.Count > 0)
                {
                    await _backlogCommentRepository.InsertAsync(taskIds
                        .Select(id => Comment(id, pusher?.Id ?? branch.CreatedById, $"Branch {branchName} was deleted in {repository.Capabilities.Name} by {pusherName}"))
                        .ToList());
                    foreach (var id in taskIds)
                        await _taskCodeService.ClearCacheAsync(id);
                }
            }
            if (existing.Count > 0)
                await _gitBranchRepository.UpdateAsync(existing);
            return;
        }

        if (!push.IsNewBranch || existing.Count > 0)
            return;

        var nameIds = BranchNameBuilder.ExtractTaskIds(branchName, _integrationSettings.BranchTaskKeyPrefix);
        var tasks = nameIds.Count == 0
            ? []
            : await _backlogRepository.Table
                .Where(x => nameIds.Contains(x.Id) && !x.Deleted && x.ProjectId == projectId)
                .ToListAsync();

        var createdById = pusher?.Id ?? tasks.Select(t => t.AssigneeId ?? t.CreatedById).FirstOrDefault();
        if (createdById <= 0)
            return;

        var newBranch = new GitBranch
        {
            ProjectId = projectId,
            ProjectRepositoryId = repoId,
            GitProjectReference = repository.RepoRef,
            Name = branchName,
            Origin = (int)GitBranchOrigin.Pushed,
            CreatedById = createdById,
            CreatedOn = DateTime.UtcNow
        };
        await _gitBranchRepository.InsertAsync(newBranch);

        if (tasks.Count > 0)
            await LinkAsync(newBranch, tasks, createdById,
                ids => $"Branch {branchName} pushed by {pusherName} and linked automatically from its name{SharedText(ids)}");
    }

    public async Task<Dictionary<int, List<string>>> GetActiveBranchNamesAsync(IList<int> backlogIds)
    {
        if (backlogIds == null || backlogIds.Count == 0)
            return [];

        var rows = await _gitBranchTaskRepository.Table
            .Where(x => backlogIds.Contains(x.BacklogId) && !x.GitBranch.Deleted)
            .Select(x => new { x.BacklogId, x.GitBranch.Name })
            .ToListAsync();

        return rows.GroupBy(x => x.BacklogId).ToDictionary(g => g.Key, g => g.Select(x => x.Name).Distinct().ToList());
    }

    public async Task<IList<int>> GetBranchCaretakerProjectIdsAsync(int userId)
    {
        var mapped = await _userProjectMapRepository.Table
            .Where(x => x.UserId == userId && (x.CanEditOthersTask || x.CanReviewMergeRequests || x.CanMergeMergeRequests))
            .Select(x => x.ProjectId)
            .ToListAsync();

        return await _projectRepository.Table
            .Where(x => x.Active && !x.Deleted
                        && (x.TeamLeadId == userId || x.CodeReviewerId == userId || mapped.Contains(x.Id)))
            .Select(x => x.Id)
            .ToListAsync();
    }

    public async Task<IList<GitBranch>> GetUnlinkedBranchesAsync(IList<int> projectIds)
    {
        if (projectIds == null || projectIds.Count == 0)
            return [];

        return await _gitBranchRepository.Table
            .Where(x => !x.Deleted && projectIds.Contains(x.ProjectId) && !x.Tasks.Any())
            .Include(x => x.Project)
            .Include(x => x.CreatedBy)
            .OrderByDescending(x => x.CreatedOn)
            .ToListAsync();
    }

    #endregion

    #region Utilities

    private async Task<GitRepositoryContext?> ResolveRepositoryAsync(int projectId, int? repositoryId)
    {
        var contexts = await _gitRepositoryService.GetProjectContextsAsync(projectId);
        return repositoryId > 0 ? contexts.FirstOrDefault(c => c.RepositoryId == repositoryId) : contexts.FirstOrDefault();
    }

    private async Task<(Backlog? Task, GitBranch? Branch, GitRepositoryContext? Repository, List<Backlog> LinkedTasks, string? Error)> PrepareMergeRequestAsync(int backlogId, int gitBranchId)
    {
        var task = await _backlogRepository.GetByIdAsync(backlogId);
        var user = await _workContext.GetCurrentUserAsync();
        if (task == null || user == null)
            return (null, null, null, [], "Task not found.");

        var permission = await CheckTaskAsync(task, user.Id);
        if (permission != null)
            return (null, null, null, [], permission);

        var branch = await _gitBranchRepository.GetByIdAsync(gitBranchId);
        if (branch == null || branch.Deleted)
            return (null, null, null, [], "This branch no longer exists.");

        var linkedIds = await _gitBranchTaskRepository.Table
            .Where(x => x.GitBranchId == branch.Id)
            .Select(x => x.BacklogId)
            .ToListAsync();
        if (!linkedIds.Contains(task.Id))
            return (null, null, null, [], "This branch is not linked to the task.");

        var repository = await _gitRepositoryService.GetBranchContextAsync(branch);
        if (repository == null || repository.Repository.ProjectId != task.ProjectId)
            return (null, null, null, [], "The repository of this branch is no longer linked to the project.");

        var forTasks = branch.Origin == (int)GitBranchOrigin.Created ? linkedIds : [task.Id];
        var linkedTasks = await _backlogRepository.Table
            .Where(x => forTasks.Contains(x.Id) && !x.Deleted)
            .OrderBy(x => x.Id)
            .ToListAsync();

        return (task, branch, repository, linkedTasks, null);
    }

    private static string MergeRequestTitle(List<Backlog> tasks)
    {
        if (tasks.Count == 0)
            return string.Empty;

        var ids = string.Join(" ", tasks.Take(3).Select(t => $"#{t.Id}")) + (tasks.Count > 3 ? $" +{tasks.Count - 3}" : string.Empty);
        var title = $"{ids} {tasks[0].Title?.Trim()}";
        return title.Length <= 250 ? title : title[..250];
    }

    private static string MergeRequestDescription(List<Backlog> tasks)
    {
        return string.Join("\n\n---\n\n", tasks.Select(t =>
        {
            var body = HtmlText.ToPlainText(t.Description, 1500);
            return $"**Task #{t.Id}: {t.Title?.Trim()}**" + (string.IsNullOrWhiteSpace(body) ? string.Empty : $"\n\n{body}");
        }));
    }

    private async Task<(User User, string UserName)?> ReviewerForAsync(Project? project, User? author, GitRepositoryContext repository)
    {
        if (project == null)
            return null;

        var authorName = await _gitAccountService.GetUserNameAsync(author, repository.Connection);
        foreach (var candidate in new[] { project.TeamLead, project.CodeReviewer })
        {
            if (candidate == null || candidate.Id == author?.Id)
                continue;

            var userName = await _gitAccountService.GetUserNameAsync(candidate, repository.Connection);
            if (string.IsNullOrWhiteSpace(userName) || string.Equals(userName, authorName, StringComparison.OrdinalIgnoreCase))
                continue;

            return (candidate, userName);
        }

        return null;
    }

    private async Task<(string? Token, string? Error)> PersonalTokenAsync(GitRepositoryContext repository, string action)
    {
        var user = await _workContext.GetCurrentUserAsync();
        return await _gitAccountService.RequireTokenAsync(user, repository.Connection, action);
    }

    private async Task<string?> CheckTaskAsync(Backlog task, int userId, bool requireOpen = true)
    {
        if (task.Deleted)
            return "Task not found.";

        if ((await _gitRepositoryService.GetProjectRepositoriesAsync(task.ProjectId)).Count == 0)
            return $"Project {task.Project?.Name} is not linked to a repository.";

        if (requireOpen)
        {
            var group = task.Status?.GroupId ?? 0;
            if (group == StatusGroup.Resolved.ToInt() || group == StatusGroup.Archived.ToInt())
                return "Only open tasks can be linked to a branch.";
        }

        if (task.AssigneeId == userId || task.Project?.TeamLeadId == userId || task.Project?.CodeReviewerId == userId)
            return null;

        var mapping = await _userService.GetProjectMapping(userId, task.ProjectId);
        return mapping != null && mapping.CanEditOthersTask
            ? null
            : "Only the assigned developer, the project's Team Lead or Code Reviewer, or someone who can edit others' tasks in this project, can manage branches.";
    }

    private async Task<(List<Backlog> Eligible, List<BranchContextDto.SkippedTask> Skipped, GitRepositoryContext? Repository, IList<GitRepositoryContext> Repositories)> EvaluateAsync(IList<int> backlogIds, int? repositoryId)
    {
        var user = await _workContext.GetCurrentUserAsync();
        var eligible = new List<Backlog>();
        var skipped = new List<BranchContextDto.SkippedTask>();
        int? projectId = null;

        foreach (var id in (backlogIds ?? []).Where(x => x > 0).Distinct().OrderBy(x => x))
        {
            var task = await _backlogRepository.GetByIdAsync(id);
            var reason = task == null ? "Task not found." : await CheckTaskAsync(task, user.Id);

            if (reason == null)
            {
                projectId ??= task!.ProjectId;
                if (projectId != task!.ProjectId)
                    reason = "This task belongs to a different project than the other selected tasks.";
            }

            if (reason == null)
                eligible.Add(task!);
            else
                skipped.Add(new BranchContextDto.SkippedTask { TaskId = id, Reason = reason });
        }

        if (projectId == null)
            return (eligible, skipped, null, []);

        var repositories = await _gitRepositoryService.GetProjectContextsAsync(projectId.Value);
        var repository = repositoryId > 0 ? repositories.FirstOrDefault(r => r.RepositoryId == repositoryId) : repositories.FirstOrDefault();
        return (eligible, skipped, repository, repositories);
    }

    private async Task<Dictionary<int, string>> ExistingWorkAsync(IList<Backlog> tasks)
    {
        var result = new Dictionary<int, string>();
        if (tasks.Count == 0)
            return result;

        var ids = tasks.Select(t => t.Id).ToList();
        var branches = await _gitBranchTaskRepository.Table
            .Where(x => ids.Contains(x.BacklogId) && !x.GitBranch.Deleted)
            .Select(x => new { x.BacklogId, x.GitBranch.Name })
            .ToListAsync();
        foreach (var g in branches.GroupBy(x => x.BacklogId))
            result[g.Key] = $"already has branch {string.Join(", ", g.Select(x => x.Name).Distinct())}";

        var reviewStatusIds = new List<int>();
        foreach (var (configured, name) in new[] { (_integrationSettings.OpenedStatusId, MergeStatusDefaults.Opened), (_integrationSettings.MergedStatusId, MergeStatusDefaults.Merged) })
        {
            var status = configured > 0 ? await _statusService.GetByIdAsync(configured.Value) : await _statusService.GetByNameAsync(name);
            if (status != null)
                reviewStatusIds.Add(status.Id);
        }

        foreach (var t in tasks.Where(t => !result.ContainsKey(t.Id) && reviewStatusIds.Contains(t.StatusId)))
            result[t.Id] = $"is already in review ({t.Status?.Name})";

        return result;
    }

    private async Task<GitBranch?> FindBranchAsync(GitRepositoryContext repository, string name)
    {
        var repoId = repository.RepositoryId;
        var repoRef = repository.RepoRef;
        return await _gitBranchRepository.Table
            .FirstOrDefaultAsync(x => !x.Deleted && x.Name == name
                                      && (x.ProjectRepositoryId == repoId || (x.ProjectRepositoryId == null && x.GitProjectReference == repoRef)));
    }

    private async Task<List<int>> LinkAsync(GitBranch branch, List<Backlog> tasks, int userId, Func<List<int>, string> note)
    {
        var existing = await _gitBranchTaskRepository.Table
            .Where(x => x.GitBranchId == branch.Id)
            .Select(x => x.BacklogId)
            .ToListAsync();

        var toLink = tasks.Where(t => !existing.Contains(t.Id)).Select(t => t.Id).ToList();
        if (toLink.Count == 0)
            return toLink;

        await _gitBranchTaskRepository.InsertAsync(toLink.Select(id => new GitBranchTask
        {
            GitBranchId = branch.Id,
            BacklogId = id,
            LinkedById = userId,
            LinkedOn = DateTime.UtcNow
        }).ToList());

        var allOnBranch = existing.Concat(toLink).Distinct().ToList();
        await _backlogCommentRepository.InsertAsync(toLink
            .Select(id => Comment(id, userId, note(allOnBranch.Where(x => x != id).ToList())))
            .ToList());

        foreach (var id in allOnBranch)
            await _taskCodeService.ClearCacheAsync(id);

        return toLink;
    }

    private static string RepositoryText(GitRepositoryContext repository) => $" in {repository.Name}";

    private static string SharedText(List<int> others) =>
        others.Count == 0 ? string.Empty : $" (shared with {string.Join(", ", others.OrderBy(x => x).Select(x => $"#{x}"))})";

    private static BacklogComment Comment(int taskId, int userId, string text) => new()
    {
        BacklogId = taskId,
        CreatedById = userId,
        CreatedOn = DateTime.UtcNow,
        Comment = text,
        SystemComment = true
    };

    #endregion
}
