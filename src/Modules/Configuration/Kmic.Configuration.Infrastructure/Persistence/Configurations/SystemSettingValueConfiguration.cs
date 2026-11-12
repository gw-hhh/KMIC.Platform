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
        builder.ToTable(t => t.HasComment("被修改过的系统设置值；Configuration 模块；一个设置一行，没有这一行就用代码里的默认值"));
        builder.Property(v => v.Key).HasMaxLength(200);
        builder.HasIndex(v => v.Key).IsUnique().MapsToError(CommonErrors.VersionConflict);
        builder.Property(v => v.Value).HasColumnType("jsonb");
        builder.HasNoDataScope("配置数据对所有登录用户一致，按权限点管理");
    }
}
