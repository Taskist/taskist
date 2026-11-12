using Taskist.Core.Domain.Masters;
using Taskist.Core.Domain.Users;
using Taskist.Core.Enums;
using Taskist.Service.Authentication;
using Taskist.Service.Localization;
using Taskist.Service.Users;
using Taskist.Web.Infrastructure;

namespace Taskist.Web.Helpers.Sso;

public class SsoSignInService : ISsoSignInService
{
    #region Fields

    private readonly OidcOptions _options;
    private readonly IUserService _userService;
    private readonly IExternalLoginService _externalLoginService;
    private readonly ILanguageService _languageService;
    private readonly ILogger<SsoSignInService> _logger;

    #endregion

    #region Ctor

    public SsoSignInService(OidcOptions options,
        IUserService userService,
        IExternalLoginService externalLoginService,
        ILanguageService languageService,
        ILogger<SsoSignInService> logger)
    {
        _options = options;
        _userService = userService;
        _externalLoginService = externalLoginService;
        _languageService = languageService;
        _logger = logger;
    }

    #endregion

    #region Methods

    public async Task<SsoSignInResult> ResolveAsync(ExternalIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);

        var link = await _externalLoginService.GetAsync(identity.Issuer, identity.Subject);
        var linkedUser = link == null ? null : await _userService.GetByIdAsync(link.UserId);

        User? emailUser = null;
        var emailUserLinked = false;
        if (linkedUser == null && !string.IsNullOrWhiteSpace(identity.Email))
        {
            emailUser = await _userService.GetByEmailAsync(identity.Email);
            if (emailUser != null)
                emailUserLinked = (await _externalLoginService.GetForUserAsync(emailUser.Id))
                    .Any(l => string.Equals(l.Provider, identity.Issuer, StringComparison.Ordinal));
        }

        var decision = SsoSignInPlanner.Decide(identity,
            hasLinkedUser: linkedUser != null,
            hasUserWithEmail: emailUser != null,
            emailUserLinkedToThisProvider: emailUserLinked,
            requireVerifiedEmail: _options.RequireVerifiedEmail,
            autoProvision: _options.AutoProvision);

        if (SsoSignInPlanner.IsRejection(decision))
            return SsoSignInResult.Fail(SsoErrorCode.FromDecision(decision) ?? SsoErrorCode.Failed);

        User? user;
        var linked = false;
        var provisioned = false;

        switch (decision)
        {
            case SsoSignInDecision.UseLinkedUser:
                user = linkedUser;
                break;
            case SsoSignInDecision.LinkExistingUser:
                user = emailUser;
                break;
            default:
                user = await ProvisionAsync(identity);
                provisioned = user != null;
                break;
        }

        if (user == null)
            return SsoSignInResult.Fail(SsoErrorCode.Failed);

        if (!await IsUsableAsync(user))
            return SsoSignInResult.Fail(SsoErrorCode.Unavailable);

        if (link == null)
        {
            await _externalLoginService.InsertAsync(new UserExternalLogin
            {
                UserId = user.Id,
                Provider = identity.Issuer,
                Subject = identity.Subject,
                Email = identity.Email,
                CreatedOn = DateTime.UtcNow,
                LastUsedOn = DateTime.UtcNow
            });
            linked = true;
        }
        else
        {
            await _externalLoginService.MarkUsedAsync(link, identity.Email);
        }

        var rolesAdded = await ApplyRoleMappingsAsync(user, identity);

        return new SsoSignInResult
        {
            User = user,
            Linked = linked && !provisioned,
            Provisioned = provisioned,
            RolesAdded = rolesAdded
        };
    }

    #endregion

    #region Utilities

    private async Task<bool> IsUsableAsync(User user)
    {
        if (user.Deleted || user.Locked || user.Status != (int)UserStatus.Active)
            return false;

        return await _userService.IsRegisteredAsync(user);
    }

    private async Task<User?> ProvisionAsync(ExternalIdentity identity)
    {
        var languages = await _languageService.GetAllActiveAsync();
        var language = languages.FirstOrDefault();
        if (language == null)
        {
            _logger.LogWarning("SSO could not create a user because no active language exists.");
            return null;
        }

        var (firstName, lastName) = ExternalNameSplitter.Split(identity);
        var user = new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = identity.Email!.Trim(),
            MobileNumber = string.Empty,
            DateOfJoin = DateOnly.FromDateTime(DateTime.UtcNow),
            LastActivityDate = DateTime.UtcNow
        };
        var address = new Address { Address1 = string.Empty, ZipPostalCode = string.Empty };

        var result = await _userService.RegisterAsync(user, address, [], new User { LanguageId = language.Id }, emailWelcomeKit: false);
        if (result != RegistrationResult.Successful)
        {
            _logger.LogWarning("SSO could not create a user for a new sign-in (result {Result}).", result);
            return null;
        }

        return await _userService.GetByEmailAsync(user.Email);
    }

    private async Task<IReadOnlyList<string>> ApplyRoleMappingsAsync(User user, ExternalIdentity identity)
    {
        var mapped = OidcRoleMapper.MapRoles(identity.Groups, _options.RoleMappings);
        if (mapped.Count == 0)
            return [];

        var current = (await _userService.GetUserRolesAsync(user, showHidden: true)).Select(r => r.SystemName);
        var toAdd = OidcRoleMapper.RolesToAdd(mapped, current);
        if (toAdd.Count == 0)
            return [];

        var roles = await _userService.GetAllUserRolesAsync();
        var added = new List<string>();
        foreach (var roleName in toAdd)
        {
            var role = roles.FirstOrDefault(r => string.Equals(r.SystemName, roleName, StringComparison.OrdinalIgnoreCase));
            if (role == null)
            {
                _logger.LogWarning("Authentication:Oidc:RoleMappings refers to role '{Role}', which does not exist or is inactive.", roleName);
                continue;
            }

            await _userService.AddUserRoleMappingAsync(new UserRoleMap { UserId = user.Id, UserRoleId = role.Id });
            added.Add(role.SystemName);
        }

        return added;
    }

    #endregion
}
