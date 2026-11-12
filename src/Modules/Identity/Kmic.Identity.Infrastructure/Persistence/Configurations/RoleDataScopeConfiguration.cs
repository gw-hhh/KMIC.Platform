using Kmic.Identity.Domain.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Identity.Infrastructure.Persistence.Configurations;

internal sealed class RoleDataScopeConfiguration : IEntityTypeConfiguration<RoleDataScope>
{
    public void Configure(EntityTypeBuilder<RoleDataScope> builder)
    {
        builder.ToTable(t => t.HasComment("角色的数据范围覆盖；Identity 模块；一行代表角色对一个范围对象单独设置的等级，随角色一起保存"));
        builder.Property(s => s.ScopeObject).HasMaxLength(200);
    }
}
