using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Identity.Domain.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Identity.Infrastructure.Persistence.Configurations;

internal sealed class OrgTreeConfiguration : IEntityTypeConfiguration<OrgTree>
{
    public void Configure(EntityTypeBuilder<OrgTree> builder)
    {
        builder.ToTable(t => t.HasComment("组织树；Identity 模块；只有一行，整棵树一个聚合；并发版本号即组织树版本号"));
        builder.HasMany(t => t.Units).WithOne().HasForeignKey("TreeId");
        builder.HasNoDataScope("身份与授权数据按权限点管理，不按归属过滤");
    }
}
