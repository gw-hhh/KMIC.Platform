using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Identity.Infrastructure.Persistence.Configurations;

internal sealed class UserRoleGrantConfiguration : IEntityTypeConfiguration<UserRoleGrant>
{
    public void Configure(EntityTypeBuilder<UserRoleGrant> builder)
    {
        builder.ToTable(t => t.HasComment("用户的角色授予；Identity 模块；一行代表用户拥有一个角色，随用户一起保存"));
        builder.HasIndex("UserId", nameof(UserRoleGrant.RoleId)).IsUnique().MapsToError(IdentityErrors.GrantExists);
        builder.HasIndex(g => g.RoleId);
        builder.HasMany(g => g.Anchors).WithOne().HasForeignKey("GrantId");
    }
}
