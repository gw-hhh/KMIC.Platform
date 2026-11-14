using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Files.Domain.Files;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Files.Infrastructure.Persistence.Configurations;

internal sealed class StorageIssueConfiguration : IEntityTypeConfiguration<StorageIssue>
{
    public void Configure(EntityTypeBuilder<StorageIssue> builder)
    {
        builder.ToTable(t => t.HasComment("存储问题（Files）：一条记录对应一次对象缺失问题；登记处理结果，不执行还原，文件记录清理后仍保留"));
        builder.Property(i => i.ObjectVersionId).HasMaxLength(200);
        builder.Property(i => i.ResolutionNote).HasMaxLength(500);

        // 不建外键，文件清理后仍保留存储问题记录
        builder.HasIndex(i => new { i.Status, i.FoundAt });
        builder.HasIndex(i => i.FileId);
        builder.HasNoDataScope("文件元数据按权限点管理；业务访问权限由业务对象决定");
    }
}
