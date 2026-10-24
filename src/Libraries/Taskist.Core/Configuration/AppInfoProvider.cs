namespace Taskist.Core.Configuration;

public class AppInfoProvider : IAppInfoProvider
{
    public AppInfoProvider(string version, bool isBeta, string betaMessage)
    {
        Version = string.IsNullOrWhiteSpace(version) ? "1.0.0" : version;
        IsBeta = isBeta;
        BetaMessage = string.IsNullOrWhiteSpace(betaMessage)
            ? "You're on a beta build — some features may change."
            : betaMessage;
    }

    public string Version { get; }

    public bool IsBeta { get; }

    public string BetaMessage { get; }
}
