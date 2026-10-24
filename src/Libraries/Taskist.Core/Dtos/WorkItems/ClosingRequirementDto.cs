using Taskist.Core.Dtos.Git;

namespace Taskist.Core.Dtos.WorkItems;

public class ClosingRequirementDto
{
    public int TaskTypeGroup { get; set; }

    public string? StatusName { get; set; }

    public bool RootCauseRequired { get; set; }

    public bool ResolutionRequired { get; set; }

    public bool MrRequired { get; set; }

    public bool HasRootCause { get; set; }

    public bool HasResolution { get; set; }

    public bool HasMr { get; set; }

    public int? ClosingMergeRequestId { get; set; }

    public bool MrLookupFailed { get; set; }

    public string? MrLookupMessage { get; set; }

    public List<TaskCodeDto.MergeRequest> LinkedMergeRequests { get; set; } = [];

    public bool RootCauseMissing => RootCauseRequired && !HasRootCause;

    public bool ResolutionMissing => ResolutionRequired && !HasResolution;

    public bool MrMissing => MrRequired && !HasMr;

    public bool AnyRequired => RootCauseRequired || ResolutionRequired || MrRequired;

    public bool AnyMissing => RootCauseMissing || ResolutionMissing || MrMissing;
}
