using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Domain.Enumerations;
using Kmic.Identity.Domain.Roles;
using Shouldly;

namespace Kmic.Identity.Application.Tests.Roles;

public sealed class RoleScopeLevelTests
{
    // 角色上的等级由基础设施层按编码转成框架的 ScopeLevel（存进角色快照的是数值），两边必须一一对应
    [Fact]
    public void EveryRoleLevel_HasAFrameworkScopeLevelWithTheSameName()
    {
        var roleLevels = Enumeration<RoleScopeLevel>.All.Select(l => l.Code).Order(StringComparer.Ordinal);
        var frameworkLevels = Enum.GetNames<ScopeLevel>().Order(StringComparer.Ordinal);

        roleLevels.ShouldBe(frameworkLevels);
    }

    [Fact]
    public void TheLevels_AreFoundByTheirCodes() =>
        RoleScopeLevel.FromCode("OrgUnitAndBelow").ShouldBe(RoleScopeLevel.OrgUnitAndBelow);
}
