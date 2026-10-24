namespace Taskist.Core.Security;

public interface ISecretProtector
{
    string? Protect(string? plainText, string? purpose = null);

    string? Unprotect(string? storedValue, string? purpose = null);

    bool IsCurrentFormat(string? storedValue);
}
