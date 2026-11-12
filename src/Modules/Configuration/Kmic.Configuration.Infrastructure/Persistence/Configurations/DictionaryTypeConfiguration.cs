using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Configuration.Domain.Dictionaries;
using Kmic.Configuration.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Configuration.Infrastructure.Persistence.Configurations;

internal sealed class DictionaryTypeConfiguration : IEntityTypeConfiguration<DictionaryType>
{
    public void Configure(EntityTypeBuilder<DictionaryType> builder)
    {
        builder.ToTable(t => t.HasComment("字典类型；Configuration 模块；由代码声明、启动时同步；字典项是它的子实体，任何字典项的变化都递增它的版本号"));
        builder.Property(t => t.Code).HasMaxLength(100);
        builder.HasIndex(t => t.Code).IsUnique().MapsToError(ConfigurationErrors.DictionaryTypeCodeExists);
        builder.Property(t => t.OwnerModule).HasMaxLength(50);
        builder.Property(t => t.DisplayName).HasMaxLength(100);
        builder.HasMany(t => t.Items).WithOne().HasForeignKey("TypeId");
        builder.HasNoDataScope("配置数据对所有登录用户一致，按权限点管理");
    }
}
