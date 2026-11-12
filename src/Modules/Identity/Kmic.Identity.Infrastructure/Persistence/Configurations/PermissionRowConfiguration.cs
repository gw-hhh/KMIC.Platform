using Kmic.Identity.Infrastructure.Persistence.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Identity.Infrastructure.Persistence.Configurations;

internal sealed class PermissionRowConfiguration : IEntityTypeConfiguration<PermissionRow>
{
    public void Configure(EntityTypeBuilder<PermissionRow> builder)
    {
        builder.ToTable("Permission", t => t.HasComment("权限点目录；Identity 模块；代码里声明的权限点同步到这里，代码里删除的标为退役，退役超过保留期由每周清理任务删除"));
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();
        builder.Property(r => r.Code).HasMaxLength(150);
        builder.Property(r => r.Module).HasMaxLength(50);
        builder.Property(r => r.GroupName).HasMaxLength(100);
        builder.Property(r => r.DisplayName).HasMaxLength(100);
        builder.HasIndex(r => r.Code).IsUnique();
    }
}
