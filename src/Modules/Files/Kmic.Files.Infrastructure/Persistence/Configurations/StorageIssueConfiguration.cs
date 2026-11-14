using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Files.Domain.Files;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Files.Infrastructure.Persistence.Configurations;

internal sealed class StorageIssueConfiguration : IEntityTypeConfiguration<StorageIssue>
{
    public void Configure(EntityTypeBuilder<StorageIssue> builder)
    {
        builder.ToTable(t => t.HasComment("存储问题；Files 模块；对账发现库里有记录而存储里找不到对象的文件，待运维还原后标为已解决"));
        builder.Property(i => i.ObjectVersionId).HasMaxLength(200);
        builder.Property(i => i.ResolutionNote).HasMaxLength(500);

        // 按状态列出待处理的，对账按文件判断是否已有待处理的问题。不建外键：文件的行清理之后，问题记录仍要留着
        builder.HasIndex(i => new { i.Status, i.FoundAt });
        builder.HasIndex(i => i.FileId);
        builder.HasNoDataScope("文件元数据按权限点管理；业务访问权限由业务对象决定");
    }
}
