using Microsoft.EntityFrameworkCore;
using Spike03.Domain;

namespace Spike03.Infrastructure;

/// <summary>
/// 原型的模块上下文
/// </summary>
public sealed class CustomerDbContext(DbContextOptions<CustomerDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.AddValueObjectConventions(typeof(Customer).Assembly);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("customer");
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
    }
}
