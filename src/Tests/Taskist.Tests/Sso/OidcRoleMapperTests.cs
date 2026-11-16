using Taskist.Web.Helpers.Sso;

namespace Taskist.Tests.Sso;

public class OidcRoleMapperTests
{
    #region Fields

    private static readonly Dictionary<string, string> Mappings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["taskist-leads"] = "TeamLead",
        ["taskist-qa"] = " QualityAssurance ",
        ["taskist-admins"] = "SystemAdministrator",
        ["blank-role"] = " "
    };

    #endregion

    #region Tests

    [Fact]
    public void MapsOnlyListedGroups_CaseInsensitive()
    {
        var roles = OidcRoleMapper.MapRoles(["TASKIST-LEADS", "taskist-qa", "everyone"], Mappings);

        Assert.Equal(["TeamLead", "QualityAssurance"], roles);
    }

    [Fact]
    public void UnlistedGroups_GrantNothing()
    {
        Assert.Empty(OidcRoleMapper.MapRoles(["Administrator", "SystemAdministrator", "admins"], Mappings));
    }

    [Fact]
    public void BlankRoleValues_AreIgnored()
    {
        Assert.Empty(OidcRoleMapper.MapRoles(["blank-role"], Mappings));
    }

    [Fact]
    public void NoMappingsOrGroups_GrantNothing()
    {
        Assert.Empty(OidcRoleMapper.MapRoles(["taskist-leads"], new Dictionary<string, string>()));
        Assert.Empty(OidcRoleMapper.MapRoles(null, Mappings));
        Assert.Empty(OidcRoleMapper.MapRoles([], Mappings));
    }

    [Fact]
    public void RolesToAdd_SkipsRolesTheUserAlreadyHas()
    {
        var toAdd = OidcRoleMapper.RolesToAdd(["TeamLead", "QualityAssurance"], ["registered", "teamlead"]);

        Assert.Equal(["QualityAssurance"], toAdd);
    }

    [Fact]
    public void RolesToAdd_NeverReturnsRolesToRemove()
    {
        var toAdd = OidcRoleMapper.RolesToAdd([], ["Registered", "TeamLead"]);

        Assert.Empty(toAdd);
    }

    #endregion
}
