using Kmic.Identity.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Infrastructure.Persistence;

/// <summary>
/// 读写上下文共用的实体配置清单
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
            .ApplyConfiguration(new VerificationCodeRowConfiguration())
            .ApplyConfiguration(new PermissionRowConfiguration())
            .ApplyConfiguration(new DataScopeObjectRowConfiguration())
            .ApplyConfiguration(new CatalogStateRowConfiguration());
}
