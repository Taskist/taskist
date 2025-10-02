using Taskist.Core.Domain.Common;
using Taskist.Core.Domain.Localization;

namespace Taskist.Core.Domain.Users;

public class User : BaseEntity, ISoftDeletedEntity
{
    private ICollection<UserRoleMap> _userRoleMaps;
    private ICollection<UserRole>? _userRoles;

    public User()
    {
        Code = Guid.NewGuid();
    }

    public Guid Code { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Email { get; set; }

    public int GenderId { get; set; }

    public int LanguageId { get; set; }

    public bool SystemAccount { get; set; }

    public int FailedLoginAttempts { get; set; }

    public string? LastIPAddress { get; set; }

    public DateTime? LastLoginDate { get; set; }

    public DateTime LastActivityDate { get; set; }

    public bool Locked { get; set; }

    public int Status { get; set; }

    public bool Deleted { get; set; }

    public string Name => $"{FirstName} {LastName}";

    public bool IsAdmin { get; set; }

    #region Roles

    public virtual ICollection<UserRole> UserRoles =>
        _userRoles ??= UserRoleMaps.Select(m => m.UserRole).ToList();

    public virtual ICollection<UserRoleMap> UserRoleMaps
    {
        get => _userRoleMaps ??= [];
        protected set => _userRoleMaps = value;
    }

    public void AddToRole(UserRoleMap role)
    {
        UserRoleMaps.Add(role);
        _userRoles = null;
    }

    public void RemoveFromRole(UserRoleMap role)
    {
        UserRoleMaps.Remove(role);
        _userRoles = null;
    }

    #endregion

    public virtual Language Language { get; set; }
}