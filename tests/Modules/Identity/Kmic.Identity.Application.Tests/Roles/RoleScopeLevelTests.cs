using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Domain.Enumerations;
using Kmic.Identity.Domain.Roles;
using Shouldly;

namespace Kmic.Identity.Application.Tests.Roles;

public sealed class RoleScopeLevelTests
{
    // 角色作用域等级由基础设施层按编码映射为框架的 ScopeLevel（角色快照中存储其数值），两者须保持严格一一对应
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
