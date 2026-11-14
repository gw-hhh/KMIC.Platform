using Kmic.Operations.Domain.Messages;

namespace Kmic.Operations.Application.Messages.Routes;

/// <summary>
/// 一条订阅路由
/// </summary>
/// <param name="MessageType">消息名称</param>
/// <param name="Subscription">订阅名</param>
/// <param name="Module">订阅方模块</param>
/// <param name="Status">状态</param>
/// <param name="RegisteredBuild">第一次登记它的构建号</param>
/// <param name="RegisteredAt">第一次登记的时间</param>
/// <param name="NeedsInitialization">是否还需要初始化</param>
/// <param name="DisplayName">显示名</param>
/// <param name="RetiredReason">退役原因，待终结和已终结的才有</param>
/// <param name="FinalizedAt">终结的时间</param>
/// <param name="FinalizedBy">终结的人</param>
/// <param name="DeclaredByBuilds">存活实例中声明该订阅的构建及实例数，供终结前检查</param>
public sealed record RouteItem(
    string MessageType,
    string Subscription,
    string Module,
    MessageRouteStatus Status,
    string RegisteredBuild,
    DateTimeOffset RegisteredAt,
    bool NeedsInitialization,
    string? DisplayName,
    string? RetiredReason,
    DateTimeOffset? FinalizedAt,
    Guid? FinalizedBy,
    IReadOnlyList<RouteBuild> DeclaredByBuilds);
