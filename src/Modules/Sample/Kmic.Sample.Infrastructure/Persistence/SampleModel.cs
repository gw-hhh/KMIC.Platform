using Kmic.Sample.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Sample.Infrastructure.Persistence;

/// <summary>
/// 读写上下文共用的实体配置清单
/// </summary>
internal static class SampleModel
{
    public static void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new SpecimenConfiguration());
        modelBuilder.ApplyConfiguration(new SpecimenItemConfiguration());
        modelBuilder.ApplyConfiguration(new SpecimenAttachmentConfiguration());
    }
}
