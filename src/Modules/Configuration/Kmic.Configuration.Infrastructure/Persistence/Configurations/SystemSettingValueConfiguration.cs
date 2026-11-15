using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Configuration.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Configuration.Infrastructure.Persistence.Configurations;

internal sealed class SystemSettingValueConfiguration : IEntityTypeConfiguration<SystemSettingValue>
{
    public void Configure(EntityTypeBuilder<SystemSettingValue> builder)
    {
        builder.ToTable(t => t.HasComment("系统设置的已保存值；Configuration 模块；每个设置一行，无记录时使用代码默认值"));
        builder.Property(v => v.Key).HasMaxLength(200);
        builder.HasIndex(v => v.Key).IsUnique().MapsToError(CommonErrors.VersionConflict);
        builder.Property(v => v.Value).HasColumnType("jsonb");
        builder.HasNoDataScope("配置数据对所有登录用户一致，按权限点管理");
    }
}
