using Microsoft.EntityFrameworkCore;
using Spike03.Infrastructure;
using Testcontainers.PostgreSql;
using Xunit;

namespace Spike03.Tests;

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18").Build();

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync(TestContext.Current.CancellationToken);
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    public CustomerDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<CustomerDbContext>().UseNpgsql(_container.GetConnectionString()).Options);
}
