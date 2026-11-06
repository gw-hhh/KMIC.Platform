using Microsoft.EntityFrameworkCore;

namespace Spike06.Tests;

// 06 第 11.3 节原写法的发布 1：读新列 DisplayName，同时写旧列 Name
public sealed class DocRelease1Context(string connectionString) : DbContext
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
