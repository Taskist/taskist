namespace Taskist.Core.Enums;

public enum LoginResult
{
    Successful = 1,
    NotExist,
    WrongPassword,
    NotActive,
    NotRegistered,
    LockedOut,
    Deleted
}
