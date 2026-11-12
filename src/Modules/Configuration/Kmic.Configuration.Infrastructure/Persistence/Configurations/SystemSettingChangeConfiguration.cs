using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Configuration.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Configuration.Infrastructure.Persistence.Configurations;

internal sealed class SystemSettingChangeConfiguration : IEntityTypeConfiguration<SystemSettingChange>
{
    public void Configure(EntityTypeBuilder<SystemSettingChange> builder)
    {
        builder.ToTable(t => t.HasComment("系统设置的变更历史；Configuration 模块；只追加，运行账号没有修改与删除权限；谁改的、什么时候改的是创建人和创建时间"));
        builder.IsAppendOnly();
        builder.Property(c => c.Key).HasMaxLength(200);
        builder.Property(c => c.OldValue).HasColumnType("jsonb");
        builder.Property(c => c.NewValue).HasColumnType("jsonb");
        builder.Property(c => c.Reason).HasMaxLength(500);
        builder.Property(c => c.TraceId).HasMaxLength(32);
        builder.HasIndex(c => c.Key);
        builder.HasNoDataScope("配置数据对所有登录用户一致，按权限点管理");
    }
}
