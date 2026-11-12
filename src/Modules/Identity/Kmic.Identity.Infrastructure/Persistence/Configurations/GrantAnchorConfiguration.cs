using Kmic.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Identity.Infrastructure.Persistence.Configurations;

internal sealed class GrantAnchorConfiguration : IEntityTypeConfiguration<GrantAnchor>
{
    public void Configure(EntityTypeBuilder<GrantAnchor> builder)
    {
        builder.ToTable(t => t.HasComment("授予的锚点；Identity 模块；一行代表授予上指定的一个起点组织单元，随授予一起保存"));
        builder.HasIndex(a => a.OrgUnitId);
    }
}
