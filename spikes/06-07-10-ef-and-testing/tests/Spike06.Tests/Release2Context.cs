using Microsoft.EntityFrameworkCore;

namespace Spike06.Tests;

// 修订后的发布 2（上一版已全部退出、再次回填之后）：读新列，仍写两列
public sealed class Release2Context(string connectionString) : DbContext
{
    public DbSet<Sample> Samples => Set<Sample>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseNpgsql(connectionString).AddInterceptors(new MirrorInterceptor());

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<Sample>(sample =>
        {
            sample.ToTable("Sample", "app");
            sample.Property(s => s.DisplayName).HasColumnName("DisplayName").MirrorTo("Name");
        });
}
