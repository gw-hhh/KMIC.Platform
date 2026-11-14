using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Integration;

namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// <see cref="IReadModelRebuildAdmin"/> 的实现
/// </summary>
/// <remarks>
/// 订阅属于哪个模块、读模型叫什么，取自本实例声明的订阅目录：Operations 的实例也是同一份代码，声明的订阅是一样的
/// </remarks>
internal sealed class RebuildAdmin(SubscriptionCatalog subscriptions, RebuildRunStore runs) : IReadModelRebuildAdmin
{
    private const int RunsPerModule = 20;

    public async Task<IReadOnlyList<RebuildRunInfo>> ListAsync(CancellationToken ct)
    {
        var all = new List<RebuildRunInfo>();
        foreach (var module in subscriptions.All.Where(s => s.Rebuild is not null).Select(s => s.Module).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))
        {
            all.AddRange(await runs.ListAsync(module, RunsPerModule, ct));
        }

        return [.. all.OrderByDescending(r => r.StartedAt).ThenByDescending(r => r.RunId)];
    }

    public async Task<RebuildAdminOutcome> TriggerAsync(string subscription, CancellationToken ct)
    {
        if (Find(subscription) is not { Rebuild: { } rebuild } definition)
        {
            return RebuildAdminOutcome.UnknownSubscription;
        }

        return await runs.QueueAsync(CallOrigin.Http, definition.Module, subscription, rebuild.ReadModel.Name, ct) is null
            ? RebuildAdminOutcome.AlreadyRunning
            : RebuildAdminOutcome.Done;
    }

    public async Task<RebuildAdminOutcome> ConfirmAsync(string subscription, CancellationToken ct)
    {
        if (Find(subscription) is not { } definition)
        {
            return RebuildAdminOutcome.UnknownSubscription;
        }

        return await runs.ConfirmAsync(definition.Module, subscription, ct) ? RebuildAdminOutcome.Done : RebuildAdminOutcome.NotAwaitingConfirmation;
    }

    public async Task<RebuildAdminOutcome> AbandonAsync(string subscription, CancellationToken ct)
    {
        if (Find(subscription) is not { } definition)
        {
            return RebuildAdminOutcome.UnknownSubscription;
        }

        return await runs.AbandonAsync(definition.Module, subscription, ct) ? RebuildAdminOutcome.Done : RebuildAdminOutcome.NotAwaitingConfirmation;
    }

    private SubscriptionDefinition? Find(string subscription) =>
        subscriptions.All.FirstOrDefault(s => string.Equals(s.Name, subscription, StringComparison.Ordinal) && s.Rebuild is not null);
}
