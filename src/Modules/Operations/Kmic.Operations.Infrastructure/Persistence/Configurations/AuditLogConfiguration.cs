using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Operations.Domain.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Operations.Infrastructure.Persistence.Configurations;

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable(t => t.HasComment("审计汇总；Operations 模块；订阅各模块的审计消息按审计 ID 幂等写入，只追加，运行账号没有修改与删除权限；创建时间是汇总到这里的时间"));
        builder.IsAppendOnly();
        builder.Property(a => a.Module).HasMaxLength(50);
        builder.Property(a => a.Action).HasMaxLength(200);
        builder.Property(a => a.Origin).HasMaxLength(30);
        builder.Property(a => a.ClientIp).HasColumnType("inet");
        builder.Property(a => a.TraceId).HasMaxLength(32);
        builder.Property(a => a.Targets).HasColumnType("jsonb");
        builder.Property(a => a.Changes).HasColumnType("jsonb");
        builder.Property(a => a.Details).HasColumnType("jsonb");

        // 时间范围是必填条件，按它扫描要能走索引；其余是常用的筛选；操作对象按包含关系查
        builder.HasIndex(a => a.OccurredAt).IsDescending();
        builder.HasIndex(a => new { a.ActorUserId, a.OccurredAt }).IsDescending(false, true);
        builder.HasIndex(a => new { a.Action, a.OccurredAt }).IsDescending(false, true);
        builder.HasIndex(a => a.Targets).HasMethod("gin").HasOperators("jsonb_path_ops");
        builder.HasNoDataScope("运维数据按权限点管理，审计人员需要看到全部");
    }
}
