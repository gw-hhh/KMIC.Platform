using Kmic.Sample.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Sample.Infrastructure.Persistence;

/// <summary>
/// 本模块的实体配置清单：写入上下文和查询上下文共用，新增实体时只改这里
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
