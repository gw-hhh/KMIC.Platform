using Kmic.Sample.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Sample.Infrastructure.Persistence;

internal static class SampleModel
{
    public static void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new SpecimenConfiguration());
        modelBuilder.ApplyConfiguration(new SpecimenItemConfiguration());
        modelBuilder.ApplyConfiguration(new SpecimenAttachmentConfiguration());
        modelBuilder.ApplyConfiguration(new SpecimenCategoryConfiguration());
    }
}
