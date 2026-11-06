using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spike02.Domain;

namespace Spike02.Infrastructure;

internal sealed class SampleConfiguration : IEntityTypeConfiguration<Sample>
{
    public void Configure(EntityTypeBuilder<Sample> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Title).HasMaxLength(200);
        builder.HasIndex(s => s.OwnerId);
        builder.HasIndex(s => s.OrgUnitId);
        builder.OwnsMany(s => s.Collaborators, c =>
        {
            c.ToTable("SampleCollaborator");
            c.WithOwner().HasForeignKey(x => x.SampleId);
            c.HasKey(x => new { x.SampleId, x.UserId });
        });
        builder.Navigation(s => s.Collaborators).HasField("_collaborators");
        builder.HasQueryFilter("SoftDelete", s => !s.IsDeleted);

        builder.HasDataScope("Sample")
            .VisibleThroughRelation(s => s.Collaborators.Any(c => c.UserId == DataScopeParam.CurrentUser))
            .RestrictVisibility(s => !s.IsPrivate || s.OwnerId == DataScopeParam.CurrentUser);
    }
}
