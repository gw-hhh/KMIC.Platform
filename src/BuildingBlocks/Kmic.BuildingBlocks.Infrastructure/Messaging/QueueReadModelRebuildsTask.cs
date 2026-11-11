using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Startup;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Persistence;

namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 给还没初始化的订阅排队重建读模型
/// </summary>
/// <remarks>
/// 看的是路由表里的“需要初始化”标记，不是“这次是不是第一次登记”：登记后、排队前进程挂了，下次启动还会补上。
/// 重建完成才清标记，已经有一轮在进行时不重复排
/// </remarks>
internal sealed class QueueReadModelRebuildsTask(SubscriptionCatalog subscriptions, RebuildRunStore runs, DataSources dataSources) : IStartupTask
{
    public string Name => "Framework.QueueReadModelRebuilds";

    public int Order => -290;

    public async ValueTask<Result> ExecuteAsync(CancellationToken ct)
    {
        var declared = subscriptions.All
            .Where(s => s.Rebuild is not null)
            .GroupBy(s => s.Name, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
        if (declared.Count == 0)
        {
            return Result.Success();
        }

        var pending = await new MessageRoutes(dataSources.Transactions(CallOrigin.Startup)).ListNeedingInitializationAsync([.. declared.Keys], ct);
        foreach (var name in pending)
        {
            var definition = declared[name];
            await runs.QueueAsync(CallOrigin.Startup, definition.Module, definition.Name, definition.Rebuild!.ReadModel.Name, ct);
        }

        return Result.Success();
    }
}
