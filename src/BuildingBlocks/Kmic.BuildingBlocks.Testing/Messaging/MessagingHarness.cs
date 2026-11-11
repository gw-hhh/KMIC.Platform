using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.BuildingBlocks.Testing.Messaging;

/// <summary>
/// 在测试进程里跑完跨模块的消息
/// </summary>
public static class MessagingHarness
{
    private const int MaxRounds = 20;

    /// <summary>
    /// 反复投递、处理，直到所有模块都没有可领取的消息
    /// </summary>
    /// <remarks>
    /// 正在退避等待重试的消息不算，要测重试时自己把下次时间改到现在。先登记本宿主声明的路由
    /// </remarks>
    /// <exception cref="InvalidOperationException">跑了很多轮还有新消息，订阅之间可能在互相触发</exception>
    public static async Task DrainAsync(this IServiceProvider services, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(services);

        var tables = services.GetRequiredService<ModuleMessageTables>();
        var dispatcher = services.GetRequiredService<OutboxDispatcher>();
        var processor = services.GetRequiredService<InboxProcessor>();
        var subscriptions = services.GetRequiredService<SubscriptionCatalog>();
        await new MessageRoutes(services.GetRequiredService<DataSources>().Transactions(CallOrigin.Startup))
            .RegisterAsync(MessageRoutes.Declared(subscriptions), MessageRoutes.Retirements(subscriptions), "test", ct);

        for (var round = 0; round < MaxRounds; round++)
        {
            var handled = 0;
            foreach (var module in tables.Modules)
            {
                handled += await RepeatAsync(c => dispatcher.DeliverBatchAsync(module, c), ct);
            }

            foreach (var module in tables.Modules)
            {
                handled += await RepeatAsync(c => processor.ProcessBatchAsync(module, c), ct);
            }

            if (handled == 0)
            {
                return;
            }
        }

        throw new InvalidOperationException($"跑了 {MaxRounds} 轮还有新消息，订阅之间可能在互相触发。");
    }

    private static async Task<int> RepeatAsync(Func<CancellationToken, Task<int>> batch, CancellationToken ct)
    {
        var total = 0;
        int count;
        while ((count = await batch(ct)) > 0)
        {
            total += count;
        }

        return total;
    }
}
