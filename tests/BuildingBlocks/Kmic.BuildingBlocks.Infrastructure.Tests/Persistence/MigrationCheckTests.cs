using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Npgsql;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Persistence;

public sealed class MigrationCheckTests(ProbeTemplate template) : IAsyncLifetime
{
    private string? _empty;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private string ConnectionString => template.ConnectionString(_empty!, pooling: false);

    public async ValueTask InitializeAsync() => _empty = await template.CreateEmptyAsync(Ct);

    public async ValueTask DisposeAsync()
    {
        NpgsqlConnection.ClearAllPools();
        await template.DropAsync(_empty!, CancellationToken.None);
    }

    [Fact]
    public async Task EmptyDatabase_ListsEveryMigrationOfEveryModule()
    {
        await using var harness = new ProbeHarness(ConnectionString);

        var pending = await ModuleMigrator.PendingAsync(harness.Services, ConnectionString, Ct);

        pending.ShouldContain(name => name.StartsWith("BuildingBlocks/", StringComparison.Ordinal));
        pending.ShouldContain(name => name.StartsWith("Echo/", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Check_PendingMigrations_RefusesToStart_NamingThem()
    {
        await using var harness = new ProbeHarness(ConnectionString);

        var thrown = await Should.ThrowAsync<InvalidOperationException>(async () => await Task(harness, autoMigrate: false).ExecuteAsync(Ct));

        thrown.Message.ShouldContain("BuildingBlocks/");
        thrown.Message.ShouldContain("Kmic.MigrationRunner");
    }

    [Fact]
    public async Task Check_AutoMigrate_AppliesEverything_ThenNothingIsPending()
    {
        await using var harness = new ProbeHarness(ConnectionString);

        (await Task(harness, autoMigrate: true).ExecuteAsync(Ct)).IsSuccess.ShouldBeTrue();

        (await ModuleMigrator.PendingAsync(harness.Services, ConnectionString, Ct)).ShouldBeEmpty();

        // 再检查一次（不自动迁移）也通过
        (await Task(harness, autoMigrate: false).ExecuteAsync(Ct)).IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task Check_AutoMigrate_UsesMigrationAccountWhenConfigured()
    {
        await using var harness = new ProbeHarness(ConnectionString);

        // 运行账号连不上这个库的结构变更：自动迁移必须走迁移账号的连接串
        var runtime = template.RuntimeConnectionString(_empty!);
        var task = new MigrationCheckTask(
            harness.Services,
            Options.Create(new DatabaseOptions { ConnectionString = runtime, MigrationConnectionString = ConnectionString, AutoMigrate = true }),
            NullLogger<MigrationCheckTask>.Instance);

        (await task.ExecuteAsync(Ct)).IsSuccess.ShouldBeTrue();

        (await ModuleMigrator.PendingAsync(harness.Services, runtime, Ct)).ShouldBeEmpty();
    }

    [Fact]
    public void Task_RunsBeforeAnyOtherStartupTask() => Task(null!, false).Order.ShouldBeLessThan(-300);

    private MigrationCheckTask Task(ProbeHarness harness, bool autoMigrate) => new(
        harness?.Services ?? new ServiceCollection().BuildServiceProvider(),
        Options.Create(new DatabaseOptions { ConnectionString = ConnectionString, AutoMigrate = autoMigrate }),
        NullLogger<MigrationCheckTask>.Instance);
}
