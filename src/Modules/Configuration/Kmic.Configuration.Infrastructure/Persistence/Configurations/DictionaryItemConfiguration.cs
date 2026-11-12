using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Configuration.Domain.Dictionaries;
using Kmic.Configuration.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Configuration.Infrastructure.Persistence.Configurations;

internal sealed class DictionaryItemConfiguration : IEntityTypeConfiguration<DictionaryItem>
{
    public void Configure(EntityTypeBuilder<DictionaryItem> builder)
    {
        builder.ToTable(t => t.HasComment("字典项；Configuration 模块；业务数据里存的是它的编码；不删除只停用"));
        builder.Property(i => i.Code).HasMaxLength(50).IsCaseInsensitive();
        builder.Property(i => i.Name).HasMaxLength(200);
        builder.Property(i => i.ParentCode).HasMaxLength(50);
        builder.HasIndex("TypeId", nameof(DictionaryItem.Code)).IsUnique().MapsToError(ConfigurationErrors.DictionaryItemCodeExists);
    }
}
