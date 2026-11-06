using Microsoft.EntityFrameworkCore;
using Spike02.Domain;

namespace Spike02.Infrastructure;

/// <summary>
/// 示例模块的上下文
/// </summary>
public sealed class SampleDbContext(DbContextOptions<SampleDbContext> options, IDataScope dataScope)
    : ModuleDbContext(options, dataScope)
{
    public DbSet<Sample> Samples => Set<Sample>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.RegisterAllInVogenEfCoreConverters();

    protected override void ConfigureModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("sample");
        modelBuilder.ApplyConfiguration(new SampleConfiguration());
    }
}
