using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Operations.Domain.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Operations.Infrastructure.Persistence.Configurations;

internal sealed class ScheduledJobSettingConfiguration : IEntityTypeConfiguration<ScheduledJobSetting>
{
    public void Configure(EntityTypeBuilder<ScheduledJobSetting> builder)
    {
        builder.ToTable(t => t.HasComment("定时任务的自定义设置；Operations 模块；空值使用代码默认值，恢复默认时保留记录；框架据此同步触发器"));
        builder.Property(s => s.JobName).HasMaxLength(200);
        builder.Property(s => s.Cron).HasMaxLength(120);
        builder.HasIndex(s => s.JobName).IsUnique().MapsToError(CommonErrors.VersionConflict);
        builder.HasNoDataScope("运维数据按权限点管理");
    }
}
