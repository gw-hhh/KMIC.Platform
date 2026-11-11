using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Testing.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Persistence;

/// <summary>
/// 按入口来源选择连接池；查询可重试但借连接超时不重试；不允许自动预备语句
/// </summary>
public sealed class DataSourceTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Theory]
    [InlineData(CallOrigin.Http, "kmic-api-tx", "kmic-api-read")]
    [InlineData(CallOrigin.Inbox, "kmic-worker-tx", "kmic-worker-read")]
    [InlineData(CallOrigin.BackgroundJob, "kmic-worker-tx", "kmic-worker-read")]
    [InlineData(CallOrigin.Startup, "kmic-worker-tx", "kmic-worker-read")]
    public async Task Contexts_UsePoolOfEntryOrigin(CallOrigin origin, string transactions, string reads)
    {
        await using var harness = new ProbeHarness(database.ConnectionString);
        await using var work = harness.Begin(origin, CurrentUser.System());

        ApplicationName(work.Db).ShouldBe(transactions);
        ApplicationName((ProbeQueryDbContext)work.Query).ShouldBe(reads);
    }

    [Fact]
    public async Task OnlyQueryContext_RetriesOnFailure()
    {
        await using var harness = new ProbeHarness(database.ConnectionString);
        await using var work = harness.BeginAsSystem();

        work.Db.Database.CreateExecutionStrategy().RetriesOnFailure.ShouldBeFalse();
        ((ProbeQueryDbContext)work.Query).Database.CreateExecutionStrategy().RetriesOnFailure.ShouldBeTrue();
    }

    [Fact]
    public async Task Query_WhenReadPoolExhausted_FailsAfterOneTimeoutWithoutRetrying()
    {
        var connectionString = new NpgsqlConnectionStringBuilder(database.ConnectionString) { Timeout = 1 }.ConnectionString;
        await using var harness = new ProbeHarness(connectionString, o => o.Api.ReadMaxPoolSize = 1);
        await using var holder = await harness.Services.GetRequiredService<DataSources>().Reads(CallOrigin.Http).OpenConnectionAsync(Ct);
        await using var work = harness.Begin(CallOrigin.Http, CurrentUser.System());
        var started = TimeProvider.System.GetTimestamp();

        var error = await Should.ThrowAsync<Exception>(() => work.Query.Tickets.CountAsync(Ct));

        // EF 默认策略会把借连接超时当作暂时性故障重试，这组参数下约 65 秒才失败（原型 #15）
        QueryRetryStrategy.IsPoolExhausted(error).ShouldBeTrue(error.ToString());
        TimeProvider.System.GetElapsedTime(started).ShouldBeLessThan(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task MaxAutoPrepare_Rejected()
    {
        var connectionString = new NpgsqlConnectionStringBuilder(database.ConnectionString) { MaxAutoPrepare = 20 }.ConnectionString;
        await using var harness = new ProbeHarness(connectionString);

        var error = Should.Throw<InvalidOperationException>(harness.Services.GetRequiredService<DataSources>);
        error.Message.ShouldContain("Max Auto Prepare");
    }

    private static string? ApplicationName(DbContext context) =>
        new NpgsqlConnectionStringBuilder(context.Database.GetDbConnection().ConnectionString).ApplicationName;
}
