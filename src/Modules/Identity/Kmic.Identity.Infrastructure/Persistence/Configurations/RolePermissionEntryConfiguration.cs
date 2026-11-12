using Kmic.Identity.Domain.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Identity.Infrastructure.Persistence.Configurations;

internal sealed class RolePermissionEntryConfiguration : IEntityTypeConfiguration<RolePermissionEntry>
{
    public void Configure(EntityTypeBuilder<RolePermissionEntry> builder)
    {
        builder.ToTable(t => t.HasComment("角色的权限点；Identity 模块；一行代表角色拥有一个权限点，随角色一起保存"));
        builder.Property(p => p.PermissionCode).HasMaxLength(200);
        builder.HasIndex(p => p.PermissionCode);
    }
}
