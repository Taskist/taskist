using Taskist.Core.Domain.Settings;
using Taskist.Core.Domain.WorkItems;
using Taskist.Core.Extensions;
using Taskist.Core.Dtos.WorkItems;
using Taskist.Core.Enums;
using Taskist.Service.Masters;

namespace Taskist.Service.WorkItems;

public class ReleaseVersionService : IReleaseVersionService
{
    #region Fields

    protected readonly IReleaseService _releaseService;
    protected readonly IProjectService _projectService;
    protected readonly ISettingService _settingService;

    #endregion

    #region Ctor

    public ReleaseVersionService(IReleaseService releaseService,
        IProjectService projectService,
        ISettingService settingService)
    {
        _releaseService = releaseService;
        _projectService = projectService;
        _settingService = settingService;
    }

    #endregion

    #region Methods

    public async Task<ReleaseVersionSuggestion> GetNextAsync(ReleaseType type, IList<int> projectIds, int baseReleaseId = 0)
    {
        var ids = Normalize(projectIds);
        var (format, fullIncrement, projectFormat) = await ResolveFormatAsync(ids.Count == 1 ? ids[0] : 0, null, null);
        var releases = await GetScopeReleasesAsync(ids);

        var baseName = baseReleaseId > 0 && type == ReleaseType.Patch
            ? releases.FirstOrDefault(x => x.Id == baseReleaseId)?.Name
            : null;

        return Build(format, fullIncrement, projectFormat, releases.Select(x => x.Name).ToList(), type, baseName);
    }

    public async Task<IList<Release>> GetPatchBasesAsync(IList<int> projectIds)
    {
        var ids = Normalize(projectIds);
        var (format, _, _) = await ResolveFormatAsync(ids.Count == 1 ? ids[0] : 0, null, null);
        var full = ReleaseType.Full.ToInt();

        return (await GetScopeReleasesAsync(ids))
            .Where(x => x.ReleaseType == full && ReleaseVersionTemplate.Matches(format, x.Name))
            .OrderByDescending(x => x.ReleaseDate)
            .ThenByDescending(x => x.Id)
            .ToList();
    }

    public async Task<ReleaseVersionCheck> CheckAsync(string name, IList<int> projectIds, int excludeReleaseId = 0)
    {
        var ids = Normalize(projectIds);
        var (format, _, _) = await ResolveFormatAsync(ids.Count == 1 ? ids[0] : 0, null, null);

        return new ReleaseVersionCheck
        {
            Format = format,
            MatchesFormat = ReleaseVersionTemplate.Matches(format, name),
            Exists = await _releaseService.NameExistsInScopeAsync(name, ids, excludeReleaseId)
        };
    }

    public async Task<(ReleaseVersionSuggestion Full, ReleaseVersionSuggestion Patch)> PreviewAsync(int projectId,
        string? format, ReleaseVersionPart? fullIncrement)
    {
        var resolved = await ResolveFormatAsync(0, format ?? string.Empty, fullIncrement);
        resolved.ProjectFormat = ReleaseVersionTemplate.IsValid(format);
        var names = (await _releaseService.GetAllForAnyProjectAsync(projectId > 0 ? [projectId] : []))
            .Select(x => x.Name).ToList();

        return (Build(resolved.Format, resolved.FullIncrement, resolved.ProjectFormat, names, ReleaseType.Full),
            Build(resolved.Format, resolved.FullIncrement, resolved.ProjectFormat, names, ReleaseType.Patch));
    }

    #endregion

    #region Utilities

    private async Task<(string Format, ReleaseVersionPart FullIncrement, bool ProjectFormat)> ResolveFormatAsync(int projectId,
        string? formatOverride, ReleaseVersionPart? fullIncrementOverride)
    {
        var settings = await _settingService.LoadSettingAsync<WorkItemSettings>();

        var defaultFormat = ReleaseVersionTemplate.IsValid(settings.ReleaseVersionFormat) ? settings.ReleaseVersionFormat : ReleaseVersionTemplate.DefaultFormat;
        var format = defaultFormat;
        var fullIncrement = ToPart(settings.ReleaseFullIncrement) ?? ReleaseVersionPart.Minor;
        var projectFormat = false;

        if (projectId > 0)
        {
            var project = await _projectService.GetByIdAsync(projectId);
            if (project != null && ReleaseVersionTemplate.IsValid(project.ReleaseVersionFormat))
            {
                format = project.ReleaseVersionFormat!;
                projectFormat = true;
            }
            fullIncrement = ToPart(project?.ReleaseFullIncrement) ?? fullIncrement;
        }

        if (formatOverride != null)
        {
            var valid = ReleaseVersionTemplate.IsValid(formatOverride);
            format = valid ? formatOverride : defaultFormat;
            projectFormat = valid;
        }

        return (format, fullIncrementOverride ?? fullIncrement, projectFormat);
    }

    private async Task<IList<Release>> GetScopeReleasesAsync(IList<int> ids)
    {
        return ids.Count == 0
            ? await _releaseService.GetAllActiveAsync()
            : await _releaseService.GetAllForAnyProjectAsync(ids);
    }

    private static List<int> Normalize(IList<int> projectIds)
    {
        return (projectIds ?? []).Where(x => x > 0).Distinct().ToList();
    }

    private static ReleaseVersionSuggestion Build(string format, ReleaseVersionPart fullIncrement, bool projectFormat,
        IList<string> names, ReleaseType type, string? baseName = null)
    {
        return new ReleaseVersionSuggestion
        {
            Version = ReleaseVersionTemplate.Next(format, names, type, fullIncrement, DateOnly.FromDateTime(DateTime.Today), baseName),
            Format = format,
            ProjectFormat = projectFormat
        };
    }

    private static ReleaseVersionPart? ToPart(int? value)
    {
        return value.HasValue && Enum.IsDefined(typeof(ReleaseVersionPart), value.Value) ? (ReleaseVersionPart)value.Value : null;
    }

    #endregion
}
