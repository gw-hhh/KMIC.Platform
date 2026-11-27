using System.Diagnostics.Metrics;
using Kmic.BuildingBlocks.Application.Observability;
using Kmic.BuildingBlocks.Infrastructure.Caching;
using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Testing.Database;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Messaging;

// 监听线程建连后会触发一次全量信号唤醒，测试需先注册信号并消耗该初始唤醒
public sealed class MessagingListenerTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>
{
    private static readonly TimeSpan _long = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan _patience = TimeSpan.FromSeconds(10);

    private readonly MessagingSignals _signals = new();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Notification_WakesOnlyThatModule()
    {
        var probe = _signals.For(MessagingChannels.Outbox, "Probe");
        var other = _signals.For(MessagingChannels.Outbox, "Other");
        using var listener = Start(_long);
        await ConnectedAsync(probe, other);

        await NotifyAsync(MessagingChannels.Outbox, "Probe");

        (await probe.WaitAsync(_patience, Ct)).ShouldBeTrue();
        (await other.WaitAsync(TimeSpan.FromMilliseconds(300), Ct)).ShouldBeFalse();
    }

    [Fact]
    public async Task NoNotification_PollIntervalStillWakes()
    {
        var signal = _signals.For(MessagingChannels.Inbox, "Echo");
        using var listener = Start(TimeSpan.FromMilliseconds(300));
        await ConnectedAsync(signal);

        (await signal.WaitAsync(TimeSpan.FromSeconds(3), Ct)).ShouldBeTrue();
    }

    [Fact]
    public async Task ConnectionKilled_ReconnectsListensAgainAndPollsOnce()
    {
        var signal = _signals.For(MessagingChannels.Outbox, "Probe");
        using var listener = Start(_long);
        await ConnectedAsync(signal);

        await using (var kill = database.DataSource.CreateCommand(
            "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE application_name = @name AND datname = current_database()"))
        {
            kill.Parameters.AddWithValue("name", MessagingListener.ApplicationName);
            await kill.ExecuteNonQueryAsync(Ct);
        }

        // 断线重连后先触发一次补偿唤醒，随后恢复正常通知接收
        (await signal.WaitAsync(_patience, Ct)).ShouldBeTrue();
        await NotifyAsync(MessagingChannels.Outbox, "Probe");
        (await signal.WaitAsync(_patience, Ct)).ShouldBeTrue();
    }

    [Fact]
    public async Task Stop_EndsWaitAndThreadExits()
    {
        var signal = _signals.For(MessagingChannels.Outbox, "Probe");
        var listener = Start(_long);
        await ConnectedAsync(signal);

        listener.Dispose();

        listener.IsRunning.ShouldBeFalse();
    }

    private MessagingListener Start(TimeSpan poll)
    {
        var listener = new MessagingListener(
            Options.Create(new DatabaseOptions { ConnectionString = database.ConnectionString }),
            Options.Create(new MessagingOptions { PollInterval = poll }),
            _signals,
            new CacheInvalidator(
                NewHybridCache(),
                new DataSources(Options.Create(new DatabaseOptions { ConnectionString = database.ConnectionString })),
                Options.Create(new CacheOptions()),
                Metrics,
                NullLogger<CacheInvalidator>.Instance),
            Metrics,
            NullLogger<MessagingListener>.Instance);
        listener.Start();
        return listener;
    }


    private static FrameworkMetrics Metrics { get; } = new(new ServiceCollection().AddMetrics().BuildServiceProvider().GetRequiredService<IMeterFactory>());

    private static HybridCache NewHybridCache()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        return services.BuildServiceProvider().GetRequiredService<HybridCache>();
    }
    private static async Task ConnectedAsync(params AsyncSignal[] signals)
    {
        foreach (var signal in signals)
        {
            (await signal.WaitAsync(_patience, Ct)).ShouldBeTrue();
        }
    }

    private async Task NotifyAsync(string channel, string module)
    {
        await using var notify = database.DataSource.CreateCommand("SELECT pg_notify(@channel, @module)");
        notify.Parameters.AddWithValue("channel", channel);
        notify.Parameters.AddWithValue("module", module);
        await notify.ExecuteNonQueryAsync(Ct);
    }
}
