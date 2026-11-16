namespace Taskist.Service.Messages;

public interface IAppLinkBuilder
{
    string Build(string relativePath);
}
