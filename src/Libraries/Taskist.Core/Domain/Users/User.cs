using Taskist.Core.Domain.Common;
using Taskist.Core.Domain.Localization;
using Taskist.Core.Domain.Masters;
using Taskist.Core.Enums;

namespace Taskist.Core.Domain.Users;

public class User : BaseEntity, ISoftDeletedEntity, IAuditableEntity
{
    private ICollection<UserRoleMap> _userUserRoleMaps;
    private ICollection<UserRole> _userRoles;

    public User()
    {
        Code = Guid.NewGuid();
    }

    public Guid Code { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string? FriendlyName { get; set; }

    public string MobileNumber { get; set; }

    public string Email { get; set; }

    public int GenderId { get; set; }

    public MaritalStatus MaritalStatus { get; set; }

    public DateOnly DateOfBirth { get; set; }

    public DateOnly DateOfJoin { get; set; }

    public int LanguageId { get; set; }

    public int AddressId { get; set; }

    public string? GitUserName { get; set; }

    public int? PhotoDocumentId { get; set; }

    public bool SystemAccount { get; set; }

    public int FailedLoginAttempts { get; set; }

    [NotAudited]
    public string? LastIPAddress { get; set; }

    [NotAudited]
    public DateTime? LastLoginDate { get; set; }

    [NotAudited]
    public DateTime LastActivityDate { get; set; }

    public bool Locked { get; set; }

    public int Status { get; set; }

    public bool Deleted { get; set; }

    public string Name => string.IsNullOrWhiteSpace(FriendlyName) ? $"{FirstName} {LastName}" : FriendlyName;

    public bool IsAdmin { get; set; }

    #region Roles

    public virtual ICollection<UserRole> UserRoles =>
        _userRoles ??= UserUserRoleMaps.Select(m => m.UserRole).ToList();

    public virtual ICollection<UserRoleMap> UserUserRoleMaps
    {
        get => _userUserRoleMaps ??= [];
        protected set => _userUserRoleMaps = value;
    }

    public void AddToRole(UserRoleMap role)
    {
        UserUserRoleMaps.Add(role);
        _userRoles = null;
    }

    public void RemoveFromRole(UserRoleMap role)
    {
        UserUserRoleMaps.Remove(role);
        _userRoles = null;
    }

    #endregion

    public virtual Language Language { get; set; }

    public virtual Address Address { get; set; }
}