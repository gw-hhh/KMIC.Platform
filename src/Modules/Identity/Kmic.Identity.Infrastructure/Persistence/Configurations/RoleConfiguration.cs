using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Identity.Infrastructure.Persistence.Configurations;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable(t => t.HasComment("角色；Identity 模块；一行代表一个角色（一组权限点加数据范围）；不删除只停用；并发版本号即授权版本号"));
        builder.Property(r => r.Name).HasMaxLength(100).IsCaseInsensitive();
        builder.HasIndex(r => r.Name).IsUnique().MapsToError(IdentityErrors.RoleNameExists);
        builder.Property(r => r.Description).HasMaxLength(500);
        builder.Property(r => r.CatalogFingerprint).HasMaxLength(64);
        builder.HasMany(r => r.Permissions).WithOne().HasForeignKey("RoleId");
        builder.HasMany(r => r.DataScopes).WithOne().HasForeignKey("RoleId");
        builder.HasNoDataScope("身份与授权数据按权限点管理，不按归属过滤");
    }
}
