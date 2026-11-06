using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Spike06.Tests;

public sealed class EventContext(string connectionString, string businessTimeZone) : DbContext
{
    public DbSet<Event> Events => Set<Event>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(connectionString);
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(new KmicFunctionsExtension(businessTimeZone));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.Entity<Event>().ToTable("Event", "app");
}
