using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Files.Domain.Files;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Files.Infrastructure.Persistence.Configurations;

internal sealed class StoredFileConfiguration : IEntityTypeConfiguration<StoredFile>
{
    // 删除队列用影子属性保存领取和重试状态，列名集中定义在此
    public const string LeaseToken = nameof(LeaseToken);
    public const string LeaseUntil = nameof(LeaseUntil);
    public const string DeleteAttempts = nameof(DeleteAttempts);
    public const string NextAttemptAt = nameof(NextAttemptAt);

    public void Configure(EntityTypeBuilder<StoredFile> builder)
    {
        builder.ToTable(t => t.HasComment("文件元数据（Files）：一条记录对应一个文件，内容存于对象存储；到期清理先删对象再删记录，彻底删除保留记录"));
        builder.Property(f => f.StorageKey).HasMaxLength(300);
        builder.Property(f => f.OriginalName).HasMaxLength(200);
        builder.Property(f => f.FileType).HasMaxLength(50);
        builder.Property(f => f.ContentType).HasMaxLength(200);
        builder.Property(f => f.Sha256).HasMaxLength(64);
        builder.Property(f => f.ObjectVersionId).HasMaxLength(200);
        builder.Property(f => f.Profile).HasMaxLength(200);
        builder.Property(f => f.RejectReason).HasMaxLength(500);
        builder.Property(f => f.PurgeReason).HasMaxLength(500);
        builder.Property(f => f.HistoryPurgeNote).HasMaxLength(500);

        // 队列列按状态条件更新，不经聚合
        builder.Property<Guid?>(LeaseToken).HasComment("删除队列的领取令牌").AuditHidden();
        builder.Property<DateTimeOffset?>(LeaseUntil).HasComment("删除队列的领取到期时间");
        builder.Property<int>(DeleteAttempts).HasComment("删除队列领取次数，人工重试时清零");
        builder.Property<DateTimeOffset?>(NextAttemptAt).HasComment("删除队列下次可以领取的时间");

        builder.HasMany(f => f.References).WithOne().HasForeignKey("StoredFileId").OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(f => f.References).AutoInclude();

        // 清理按到期时间找候选；删除工作器按状态领取；管理列表按上传人、上传配置筛选
        builder.HasIndex(f => new { f.Status, f.ExpiresAt });
        builder.HasIndex(f => new { f.UploadedBy, f.UploadedAt });
        builder.HasIndex(f => f.Profile);
        builder.HasNoDataScope("文件元数据按权限点管理；业务访问权限由业务对象决定");
    }
}
