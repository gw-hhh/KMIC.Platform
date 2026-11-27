using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>订阅路由运维操作（终结、重新激活与补投）。</summary>
public interface IMessageRouteAdmin
{
    /// <summary>终结待终结的订阅路由，此后不再向该订阅投递消息。</summary>
    Task<RouteFinalizeResult> FinalizeAsync(
        string messageType, string subscription, UserId by, string reason, bool abandonUnfinished, TimeSpan instanceAliveWithin, CancellationToken ct);

    /// <summary>重新激活已终结的订阅；终结期间遗漏的消息不会自动补发，需通过补投恢复。</summary>
    Task<RouteAdminOutcome> ReactivateAsync(string messageType, string subscription, CancellationToken ct);

    /// <summary>按序号区间补投已投递消息；已终结的订阅须先重新激活后再补投。</summary>
    Task<RedeliverResult> RedeliverAsync(string sourceModule, string messageType, string subscription, long fromSeq, long toSeq, CancellationToken ct);

    /// <summary>单次补投允许的最大消息条数。</summary>
    const int MaxRedeliverRows = 5000;
}
