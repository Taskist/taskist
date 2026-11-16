namespace Taskist.Service.Git;

public class GitProviderCapabilities
{
    public string Name { get; init; } = string.Empty;

    public string Term { get; init; } = "merge request";

    public string ShortTerm { get; init; } = "MR";

    public string RefPrefix { get; init; } = "!";

    public bool CanRebase { get; init; }

    public string RebaseLabel { get; init; } = "Rebase";

    public bool CanUnapprove { get; init; }

    public string RequiredUserScopes { get; init; } = string.Empty;

    public string RequiredServiceScopes { get; init; } = string.Empty;

    public string Ref(int iid) => $"{RefPrefix}{iid}";

    public string Label(int iid) => $"{ShortTerm} {RefPrefix}{iid}";
}
