using Kmic.Identity.Infrastructure.Persistence.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Identity.Infrastructure.Persistence.Configurations;

internal sealed class DataScopeObjectRowConfiguration : IEntityTypeConfiguration<DataScopeObjectRow>
{
    public void Configure(EntityTypeBuilder<DataScopeObjectRow> builder)
    {
        builder.ToTable("DataScopeObject", t => t.HasComment("数据范围对象目录；Identity 模块；代码里声明的范围对象同步到这里，处理方式与权限点目录相同"));
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();
        builder.Property(r => r.Code).HasMaxLength(100);
        builder.Property(r => r.Module).HasMaxLength(50);
        builder.Property(r => r.DisplayName).HasMaxLength(100);
        builder.HasIndex(r => r.Code).IsUnique();
    }
}
