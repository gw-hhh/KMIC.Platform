using Microsoft.EntityFrameworkCore;

namespace Spike06.Tests;

// 修订后的发布 1：仍读旧列 Name，同时写新列 DisplayName
public sealed class FixedRelease1Context(string connectionString) : DbContext
{
    public DbSet<Sample> Samples => Set<Sample>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseNpgsql(connectionString).AddInterceptors(new MirrorInterceptor());

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<Sample>(sample =>
        {
            sample.ToTable("Sample", "app");
            sample.Property(s => s.DisplayName).HasColumnName("Name").MirrorTo("DisplayName");
        });
}
