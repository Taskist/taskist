using Taskist.Core.Dtos.Git;

namespace Taskist.Service.WorkItems;

public interface IBranchCleanupService
{
    Task<bool> CanCleanupAsync(int userId);

    Task<BranchCleanupDto> GetCandidatesAsync(int userId);

    Task<BranchCleanupResult> DeleteAsync(int userId, IList<int> branchIds);
}
