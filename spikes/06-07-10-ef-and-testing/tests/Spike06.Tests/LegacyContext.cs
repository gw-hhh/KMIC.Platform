using Microsoft.EntityFrameworkCore;

namespace Spike06.Tests;

// 上一版代码：只读写旧列 Name
public sealed class LegacyContext(string connectionString) : DbContext
{
    public DbSet<LegacySample> Samples => Set<LegacySample>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseNpgsql(connectionString).AddInterceptors(new MirrorInterceptor());

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<LegacySample>(sample =>
        {
            sample.ToTable("Sample", "app");
            sample.Property(s => s.Name).HasColumnName("Name");
        });
}
