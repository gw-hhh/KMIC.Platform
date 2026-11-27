using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.BuildingBlocks.Testing.Messaging;

/// <summary>测试进程内跨模块消息驱动辅助工具。</summary>
public static class MessagingHarness
{
    private const int MaxRounds = 20;

    /// <summary>循环投递与处理消息直至全部模块清空（不含处于退避等待期的消息；超过最大轮次仍有新消息时抛出异常）。</summary>
    public static async Task DrainAsync(this IServiceProvider services, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(services);

        var tables = services.GetRequiredService<ModuleFrameworkTables>();
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

        throw new InvalidOperationException($"已执行 {MaxRounds} 轮仍有新消息，可能存在订阅循环触发。");
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
