namespace Taskist.Core.Configuration;

public interface IAppInfoProvider
{
    string Version { get; }

    bool IsBeta { get; }

    string BetaMessage { get; }
}
