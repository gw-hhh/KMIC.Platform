using Microsoft.EntityFrameworkCore;
using Spike02.Domain;
using Spike02.Infrastructure;
using Testcontainers.PostgreSql;
using Xunit;

namespace Spike02.Tests;

// 一个 PG 18 容器 + 一组固定数据，覆盖过滤器的每个分支
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18").Build();

    public static readonly OrgUnitId South = OrgUnitId.From(new Guid("019a5c3e-0000-7000-8000-00000000b004"));

    public IReadOnlyDictionary<string, SampleId> Samples { get; private set; } = new Dictionary<string, SampleId>(StringComparer.Ordinal);

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync(TestContext.Current.CancellationToken);

        await using var context = CreateContext(FakeDataScope.System());
        await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

        var samples = new Dictionary<string, Sample>(StringComparer.Ordinal)
        {
            ["S1 我的、华东"] = New(Ids.Me, Ids.East, isPrivate: false),
            ["S2 他人、华东"] = New(Ids.Other, Ids.East, isPrivate: false),
            ["S3 他人、华西"] = New(Ids.Other, Ids.West, isPrivate: false),
            ["S4 他人、华北、私有"] = New(Ids.Other, Ids.North, isPrivate: true),
            ["S5 他人、华南、我是协作人"] = New(Ids.Other, South, isPrivate: false),
            ["S6 他人、华北、私有、我是协作人"] = New(Ids.Other, Ids.North, isPrivate: true),
            ["S7 我的、已删除"] = New(Ids.Me, Ids.East, isPrivate: false),
        };
        samples["S5 他人、华南、我是协作人"].AddCollaborator(Ids.Me);
        samples["S6 他人、华北、私有、我是协作人"].AddCollaborator(Ids.Me);
        samples["S7 我的、已删除"].Delete();

        context.Samples.AddRange(samples.Values);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        Samples = samples.ToDictionary(p => p.Key, p => p.Value.Id, StringComparer.Ordinal);
    }

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    public SampleDbContext CreateContext(IDataScope scope) => new(CreateOptions(), scope);

    public DbContextOptions<SampleDbContext> CreateOptions(Action<DbContextOptionsBuilder>? configure = null)
    {
        var builder = new DbContextOptionsBuilder<SampleDbContext>().UseNpgsql(ConnectionString);
        configure?.Invoke(builder);
        return builder.Options;
    }

    private static Sample New(UserId owner, OrgUnitId orgUnit, bool isPrivate) =>
        new(SampleId.From(Guid.CreateVersion7()), "示例", owner, orgUnit, isPrivate);
}
