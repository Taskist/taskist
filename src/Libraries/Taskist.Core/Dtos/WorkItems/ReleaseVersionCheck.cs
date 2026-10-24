namespace Taskist.Core.Dtos.WorkItems;

public class ReleaseVersionCheck
{
    public bool MatchesFormat { get; set; }

    public bool Exists { get; set; }

    public string Format { get; set; }
}
