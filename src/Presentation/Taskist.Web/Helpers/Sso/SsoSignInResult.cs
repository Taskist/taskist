using Taskist.Core.Domain.Users;

namespace Taskist.Web.Helpers.Sso;

public class SsoSignInResult
{
    #region Properties

    public User? User { get; init; }

    public string? ErrorCode { get; init; }

    public bool Linked { get; init; }

    public bool Provisioned { get; init; }

    public IReadOnlyList<string> RolesAdded { get; init; } = [];

    public bool Succeeded => User != null && ErrorCode == null;

    #endregion

    #region Methods

    public static SsoSignInResult Fail(string code) => new() { ErrorCode = code };

    #endregion
}
