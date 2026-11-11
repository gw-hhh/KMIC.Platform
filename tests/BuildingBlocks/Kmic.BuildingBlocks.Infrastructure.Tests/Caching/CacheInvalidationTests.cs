using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.BuildingBlocks.Testing.Waiting;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Caching;

/// <summary>
/// 每个测试用自己的键和标签。“另一个实例”是同一个库上的第二套服务，只启动它的监听
/// </summary>
public sealed class CacheInvalidationTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncDisposable
{
    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));
    private static readonly TestIdentity _alice = TestIdentity.User("甲").InTree(_org).InOrgUnit(_org["总部"]);

    private static int _counter;

    private readonly string _suffix = Guid.CreateVersion7().ToString("N");
    private readonly List<ProbeHarness> _harnesses = [];

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private string Tag => "Test:Tag:" + _suffix;

    private string Key => "Test:Key:" + _suffix;

    [Fact]
    public async Task Committed_TagInvalidatedOnThisInstance()
    {
        var harness = NewHarness();
        var first = await GetAsync(harness);

        (await SendAsync(harness, new InvalidateCacheCommand([Tag, Tag]))).ShouldBeSuccess();

        (await GetAsync(harness)).ShouldBeGreaterThan(first);
    }

    [Fact]
    public async Task FailedCommand_NothingInvalidated()
    {
        var harness = NewHarness();
        var first = await GetAsync(harness);

        (await SendAsync(harness, new InvalidateCacheCommand([Tag], Fail: true))).ShouldBeFailure(TicketErrors.Rejected);

        (await GetAsync(harness)).ShouldBe(first);
    }

    [Fact]
    public async Task OtherInstance_LocalCacheClearedByBroadcast()
    {
        var writer = NewHarness();
        var other = NewHarness();
        var listener = Listener(other);
        await listener.StartAsync(Ct);
        try
        {
            await ListeningAsync();
            var first = await StableValueAsync(other);

            // 订阅好通道之前发出的通知收不到，所以每次探测都再发一次，直到另一个实例的缓存被清掉
            await Eventually.UntilAsync(
                async _ =>
                {
                    (await SendAsync(writer, new InvalidateCacheCommand([Tag]))).ShouldBeSuccess();
                    other.Time.Advance(TimeSpan.FromMilliseconds(1));
                    return await GetAsync(other);
                },
                value => value > first,
                "另一个实例的本地缓存被广播清掉",
                ct: Ct);
        }
        finally
        {
            await listener.StopAsync(Ct);
        }
    }

    [Fact]
    public async Task ListenerReconnected_AllLocalEntriesCleared()
    {
        var harness = NewHarness();
        var listener = Listener(harness);
        await listener.StartAsync(Ct);
        try
        {
            await ListeningAsync();
            var first = await StableValueAsync(harness);

            await using (var kill = database.DataSource.CreateCommand(
                "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE application_name = @name AND datname = current_database()"))
            {
                kill.Parameters.AddWithValue("name", MessagingListener.ApplicationName);
                await kill.ExecuteNonQueryAsync(Ct);
            }

            await Eventually.UntilAsync(
                _ =>
                {
                    harness.Time.Advance(TimeSpan.FromMilliseconds(1));
                    return GetAsync(harness).AsTask();
                },
                value => value > first,
                "重连之后清空本地缓存",
                ct: Ct);
        }
        finally
        {
            await listener.StopAsync(Ct);
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var harness in _harnesses)
        {
            await harness.DisposeAsync();
        }
    }

    private static MessagingListenerService Listener(ProbeHarness harness) =>
        harness.Services.GetServices<IHostedService>().OfType<MessagingListenerService>().Single();

    private static async Task<Result> SendAsync(ProbeHarness harness, InvalidateCacheCommand command)
    {
        await using var work = harness.Begin(_alice);
        return await work.Sender.SendAsync(command, Ct);
    }

    private ProbeHarness NewHarness()
    {
        var harness = new ProbeHarness(database.ConnectionString);
        _harnesses.Add(harness);
        return harness;
    }

    // 本地缓存里的值每次重新生成都加一，值变大就说明缓存被清过
    private async ValueTask<int> GetAsync(ProbeHarness harness) =>
        await harness.Services.GetRequiredService<HybridCache>().GetOrCreateAsync(
            Key,
            _ => ValueTask.FromResult(Interlocked.Increment(ref _counter)),
            tags: [Tag],
            cancellationToken: Ct);

    // 监听刚连上时会清一次全部缓存，等它过去，连续两次读到同一个值才算稳定
    private async Task<int> StableValueAsync(ProbeHarness harness)
    {
        var previous = await GetAsync(harness);
        var result = await Eventually.UntilAsync(
            async _ =>
            {
                harness.Time.Advance(TimeSpan.FromMilliseconds(1));
                var current = await GetAsync(harness);
                var stable = current == previous;
                previous = current;
                return (current, stable);
            },
            probe => probe.stable,
            "监听连上后的清空已经结束",
            ct: Ct);
        return result.current;
    }

    // 监听连接建立好才发通知，否则通知会丢
    private async Task ListeningAsync() =>
        await Eventually.UntilAsync(
            async _ =>
            {
                await using var count = database.DataSource.CreateCommand(
                    "SELECT count(*) FROM pg_stat_activity WHERE application_name = @name AND datname = current_database()");
                count.Parameters.AddWithValue("name", MessagingListener.ApplicationName);
                return (long)(await count.ExecuteScalarAsync(Ct))!;
            },
            connections => connections >= 1,
            "监听连接已经建立",
            ct: Ct);
}
