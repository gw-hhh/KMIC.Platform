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

        // 同一个业务对象对同一个文件只登记一次；聚合里关联是幂等的，撞上唯一索引只可能是两个请求同时登记，重试即可
        builder.HasIndex("StoredFileId", nameof(FileReference.OwnerModule), nameof(FileReference.OwnerType), nameof(FileReference.OwnerId))
            .IsUnique()
            .MapsToError(CommonErrors.VersionConflict);

        // 按业务对象反查它引用了哪些文件
        builder.HasIndex(r => new { r.OwnerModule, r.OwnerType, r.OwnerId });
    }
}
