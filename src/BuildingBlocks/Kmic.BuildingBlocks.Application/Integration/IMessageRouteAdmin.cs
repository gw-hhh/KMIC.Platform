using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 订阅路由的运维操作：终结与重新激活
/// </summary>
public interface IMessageRouteAdmin
{
    /// <summary>
    /// 终结待终结的订阅，之后不再向它投递
    /// </summary>
    Task<RouteFinalizeResult> FinalizeAsync(
        string messageType, string subscription, UserId by, string reason, bool abandonUnfinished, TimeSpan instanceAliveWithin, CancellationToken ct);

    /// <summary>
    /// 重新激活已终结的订阅
    /// </summary>
    /// <remarks>
    /// 终结期间漏掉的消息不会补回，要用补投
    /// </remarks>
    Task<RouteAdminOutcome> ReactivateAsync(string messageType, string subscription, CancellationToken ct);

    /// <summary>
    /// 补投：按序号区间把已投递的消息再投给指定订阅
    /// </summary>
    /// <remarks>
    /// 已终结的订阅要先重新激活
    /// </remarks>
    Task<RedeliverResult> RedeliverAsync(string sourceModule, string messageType, string subscription, long fromSeq, long toSeq, CancellationToken ct);

    /// <summary>
    /// 一次补投最多多少条
    /// </summary>
    const int MaxRedeliverRows = 5000;
}
