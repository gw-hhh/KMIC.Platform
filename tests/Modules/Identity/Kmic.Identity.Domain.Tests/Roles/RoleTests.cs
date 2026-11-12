using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Roles;
using Shouldly;

namespace Kmic.Identity.Domain.Tests.Roles;

public sealed class RoleTests
{
    private static Role Seller() => Role.Create("销售", "一线销售", 20, RoleScopeLevel.OrgUnit).ShouldBeSuccess();

    [Fact]
    public void Create_StartsActive_WithoutPermissions()
    {
        var role = Seller();

        role.Status.ShouldBe(RoleStatus.Active);
        role.IsProtected.ShouldBeFalse();
        role.GrantsAllPermissions.ShouldBeFalse();
        role.Permissions.ShouldBeEmpty();
        role.DefaultScopeLevel.ShouldBe(RoleScopeLevel.OrgUnit);
        role.StateTransitions.ShouldHaveSingleItem();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_BlankName_IsRefused(string? name) =>
        Role.Create(name, null, 0, RoleScopeLevel.Self).ShouldBeFailure(IdentityErrors.RoleNameInvalid);

    [Fact]
    public void Create_TooLongNameOrDescription_IsRefused()
    {
        Role.Create(new string('名', 101), null, 0, RoleScopeLevel.Self).ShouldBeFailure(IdentityErrors.RoleNameInvalid);
        Role.Create("销售", new string('说', 501), 0, RoleScopeLevel.Self).IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void SetPermissions_ReplacesTheSet_KeepingUnchangedRows()
    {
        var role = Seller();
        role.SetPermissions(["Sample.Specimen.View", "Sample.Specimen.Create"]).ShouldBeSuccess();
        var keptId = role.Permissions.Single(p => string.Equals(p.PermissionCode, "Sample.Specimen.View", StringComparison.Ordinal)).Id;

        role.SetPermissions(["Sample.Specimen.View", "Sample.Specimen.Submit", "Sample.Specimen.Submit"]).ShouldBeSuccess();

        role.Permissions.Select(p => p.PermissionCode).ShouldBe(["Sample.Specimen.View", "Sample.Specimen.Submit"], ignoreOrder: true);
        role.Permissions.Single(p => string.Equals(p.PermissionCode, "Sample.Specimen.View", StringComparison.Ordinal)).Id.ShouldBe(keptId);
    }

    [Fact]
    public void SetDataScopes_ReplacesDefaultAndOverrides()
    {
        var role = Seller();
        role.SetDataScopes(RoleScopeLevel.OrgUnitAndBelow, new Dictionary<string, RoleScopeLevel>(StringComparer.Ordinal) { ["Sample.Specimen"] = RoleScopeLevel.All, ["Sales.Quote"] = RoleScopeLevel.Self }).ShouldBeSuccess();

        role.SetDataScopes(RoleScopeLevel.Self, new Dictionary<string, RoleScopeLevel>(StringComparer.Ordinal) { ["Sample.Specimen"] = RoleScopeLevel.OrgUnit }).ShouldBeSuccess();

        role.DefaultScopeLevel.ShouldBe(RoleScopeLevel.Self);
        var only = role.DataScopes.ShouldHaveSingleItem();
        only.ScopeObject.ShouldBe("Sample.Specimen");
        only.Level.ShouldBe(RoleScopeLevel.OrgUnit);
    }

    [Fact]
    public void Disable_Then_Enable_RoundTrips_AndRefusesRepeats()
    {
        var role = Seller();

        role.Disable("不再使用").ShouldBeSuccess();
        role.Status.ShouldBe(RoleStatus.Disabled);
        role.Disable().ShouldBeFailure(IdentityErrors.InvalidState);
        role.Enable().ShouldBeSuccess();
        role.Status.ShouldBe(RoleStatus.Active);
    }

    [Fact]
    public void Rename_And_Describe_ValidateAndApply()
    {
        var role = Seller();

        role.Rename("区域销售").ShouldBeSuccess();
        role.Describe("  ").ShouldBeSuccess();

        role.Name.ShouldBe("区域销售");
        role.Description.ShouldBeNull();
        role.Rename("").ShouldBeFailure(IdentityErrors.RoleNameInvalid);
    }

    [Fact]
    public void SystemAdministrator_IsProtected_GrantsAll_AndCannotBeEdited()
    {
        var admin = Role.CreateSystemAdministrator();

        admin.IsProtected.ShouldBeTrue();
        admin.GrantsAllPermissions.ShouldBeTrue();
        admin.IsBuiltIn.ShouldBeTrue();
        admin.DefaultScopeLevel.ShouldBe(RoleScopeLevel.All);
        admin.SetPermissions(["Sample.Specimen.View"]).ShouldBeFailure(IdentityErrors.SystemRoleImmutable);
        admin.SetDataScopes(RoleScopeLevel.Self, new Dictionary<string, RoleScopeLevel>(StringComparer.Ordinal)).ShouldBeFailure(IdentityErrors.SystemRoleImmutable);
        admin.Disable().ShouldBeFailure(IdentityErrors.SystemRoleImmutable);
        admin.Status.ShouldBe(RoleStatus.Active);
    }

    [Fact]
    public void SyncCatalog_OnlyAffectsAllPermissionRoles()
    {
        var admin = Role.CreateSystemAdministrator();
        var seller = Seller();

        admin.SyncCatalog("hash-1");
        seller.SyncCatalog("hash-1");

        admin.CatalogHash.ShouldBe("hash-1");
        seller.CatalogHash.ShouldBeNull();
    }
}
