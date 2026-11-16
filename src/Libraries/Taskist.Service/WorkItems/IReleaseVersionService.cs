using Taskist.Core.Domain.WorkItems;
using Taskist.Core.Dtos.WorkItems;
using Taskist.Core.Enums;

namespace Taskist.Service.WorkItems;

public interface IReleaseVersionService
{
    Task<ReleaseVersionSuggestion> GetNextAsync(ReleaseType type, IList<int> projectIds, int baseReleaseId = 0);

    Task<IList<Release>> GetPatchBasesAsync(IList<int> projectIds);

    Task<ReleaseVersionCheck> CheckAsync(string name, IList<int> projectIds, int excludeReleaseId = 0);

    Task<(ReleaseVersionSuggestion Full, ReleaseVersionSuggestion Patch)> PreviewAsync(int projectId,
        string? format, ReleaseVersionPart? fullIncrement);
}
