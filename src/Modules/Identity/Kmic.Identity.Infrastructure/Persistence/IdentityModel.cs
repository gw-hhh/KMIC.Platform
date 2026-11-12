using Kmic.Identity.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Infrastructure.Persistence;

/// <summary>
/// 写入上下文与查询上下文共用同一个模型
/// </summary>
internal static class IdentityModel
{
    public static void Configure(ModelBuilder modelBuilder) =>
        modelBuilder
            .ApplyConfiguration(new UserConfiguration())
            .ApplyConfiguration(new UserRoleGrantConfiguration())
            .ApplyConfiguration(new GrantAnchorConfiguration())
            .ApplyConfiguration(new RoleConfiguration())
            .ApplyConfiguration(new RolePermissionEntryConfiguration())
            .ApplyConfiguration(new RoleDataScopeConfiguration())
            .ApplyConfiguration(new OrgTreeConfiguration())
            .ApplyConfiguration(new OrgUnitConfiguration())
            .ApplyConfiguration(new SessionConfiguration())
            .ApplyConfiguration(new RefreshTokenConfiguration())
            .ApplyConfiguration(new LoginThrottleRowConfiguration())
            .ApplyConfiguration(new VerificationCodeRowConfiguration());
}
