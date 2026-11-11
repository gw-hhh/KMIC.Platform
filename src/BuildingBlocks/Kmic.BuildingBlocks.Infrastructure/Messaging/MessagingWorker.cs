using System.Data.Common;
using System.Reflection;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// Worker 角色里的投递与处理
/// </summary>
/// <remarks>
/// 每个模块一个投递循环，有订阅的模块另有几个处理循环。没有可领取的消息时等数据库通知或兜底轮询
/// </remarks>
internal sealed class MessagingWorker(
    MessagingListener listener,
    MessagingSignals signals,
    OutboxDispatcher dispatcher,
    InboxProcessor processor,
    ModuleMessageTables tables,
    SubscriptionCatalog subscriptions,
    DataSources dataSources,
    TimeProvider time,
    IOptions<MessagingOptions> options,
    ILogger<MessagingWorker> logger) : BackgroundService
{
    private static readonly TimeSpan _maxDatabaseDelay = TimeSpan.FromSeconds(60);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 路由要先登记再投递，这样新订阅从第一条消息起就收得到。启动任务就绪后挪过去
        await new MessageRoutes(dataSources.Transactions(CallOrigin.Startup)).RegisterAsync(
            MessageRoutes.Declared(subscriptions), MessageRoutes.Retirements(subscriptions), Build, stoppingToken);

        // 停机时先不再领取；进行中的处理给一段时间做完，超时才取消，事务回滚后由别的实例接管
        using var processing = new CancellationTokenSource();
        await using var grace = stoppingToken.Register(() => processing.CancelAfter(options.Value.ShutdownGrace));

        var loops = new List<Task>();
        foreach (var module in tables.Modules)
        {
            loops.Add(RunAsync(ct => dispatcher.DeliverBatchAsync(module, ct), signals.For(MessagingChannels.Outbox, module), stoppingToken, processing.Token));
            if (subscriptions.All.Any(s => string.Equals(s.Module, module, StringComparison.Ordinal)))
            {
                for (var i = 0; i < options.Value.InboxParallelism; i++)
                {
                    loops.Add(RunAsync(ct => processor.ProcessBatchAsync(module, ct), signals.For(MessagingChannels.Inbox, module), stoppingToken, processing.Token));
                }
            }
        }

        listener.Start();
        try
        {
            await Task.WhenAll(loops);
        }
        finally
        {
            listener.Stop();
        }
    }

    private static string Build =>
        Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown";

    private async Task RunAsync(Func<CancellationToken, Task<int>> batch, AsyncSignal signal, CancellationToken stopping, CancellationToken processing)
    {
        var failures = 0;
        while (!stopping.IsCancellationRequested)
        {
            int claimed;
            try
            {
                claimed = await batch(processing);
                failures = 0;
            }
            catch (DbException exception)
            {
                // 数据库不可用时退避，恢复后立即接着领取
                failures++;
                var delay = TimeSpan.FromSeconds(Math.Min(Math.Pow(2, failures - 1), _maxDatabaseDelay.TotalSeconds));
                MessagingLog.LoopDatabaseUnavailable(logger, exception, delay.TotalSeconds);
                await DelayAsync(delay, stopping);
                continue;
            }
            catch (OperationCanceledException) when (stopping.IsCancellationRequested)
            {
                return;
            }

            if (claimed == 0)
            {
                await signal.WaitAsync(options.Value.PollInterval, stopping);
            }
        }
    }

    private async Task DelayAsync(TimeSpan delay, CancellationToken ct)
    {
        try
        {
            await Task.Delay(delay, time, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
        }
    }
}
