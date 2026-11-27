using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Files.Domain.Files;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Files.Infrastructure.Persistence.Configurations;

internal sealed class FileReferenceConfiguration : IEntityTypeConfiguration<FileReference>
{
    public void Configure(EntityTypeBuilder<FileReference> builder)
    {
        builder.ToTable(t => t.HasComment("文件的引用登记；Files 模块；业务对象对文件的引用，只由关联与解除关联的请求消息维护，用来判断文件的生命周期"));
        builder.Property(r => r.OwnerModule).HasMaxLength(50);
        builder.Property(r => r.OwnerType).HasMaxLength(100);

        // 同一业务对象对同一文件仅登记一次；并发重复登记由唯一索引兜底，返回版本冲突后可重试
        builder.HasIndex("StoredFileId", nameof(FileReference.OwnerModule), nameof(FileReference.OwnerType), nameof(FileReference.OwnerId))
            .IsUnique()
            .MapsToError(CommonErrors.VersionConflict);

        // 支持按业务对象反查所引用的文件
        builder.HasIndex(r => new { r.OwnerModule, r.OwnerType, r.OwnerId });
    }
}
