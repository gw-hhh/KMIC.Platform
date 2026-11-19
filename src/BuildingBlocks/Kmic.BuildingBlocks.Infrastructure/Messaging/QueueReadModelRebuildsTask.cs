using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Startup;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Persistence;

namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 给还没初始化的订阅排队重建读模型
/// </summary>
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
