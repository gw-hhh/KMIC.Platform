using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Identity.Infrastructure.Persistence.Configurations;

internal sealed class OrgUnitConfiguration : IEntityTypeConfiguration<OrgUnit>
{
    public void Configure(EntityTypeBuilder<OrgUnit> builder)
    {
        builder.ToTable(t => t.HasComment("组织单元；Identity 模块；一行代表组织树上的一个节点，随组织树一起保存"));
        builder.Property(u => u.Name).HasMaxLength(100).IsCaseInsensitive();
        builder.Property(u => u.Code).HasMaxLength(50);
        builder.HasIndex(u => new { u.ParentId, u.Name }).IsUnique().MapsToError(IdentityErrors.OrgUnitNameExists);
    }
}
