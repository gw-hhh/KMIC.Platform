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
        builder.ToTable(t => t.HasComment("定时任务的覆盖值；Operations 模块；运维改过的开关与执行时间，只存改过的项，没改的项按代码里的默认值；框架同步触发器时读这张表"));
        builder.Property(s => s.JobName).HasMaxLength(200);
        builder.Property(s => s.Cron).HasMaxLength(120);
        builder.HasIndex(s => s.JobName).IsUnique().MapsToError(CommonErrors.VersionConflict);
        builder.HasNoDataScope("运维数据按权限点管理");
    }
}
