using Kmic.Operations.Domain.Messages;

namespace Kmic.Operations.Application.Messages.Routes;

/// <summary>订阅路由项。</summary>
/// <param name="MessageType">消息名称</param>
/// <param name="Subscription">订阅名</param>
/// <param name="Module">订阅方模块</param>
/// <param name="Status">状态</param>
/// <param name="RegisteredBuild">首次登记构建号</param>
/// <param name="RegisteredAt">首次登记时间</param>
/// <param name="NeedsInitialization">是否仍需初始化</param>
/// <param name="DisplayName">显示名</param>
/// <param name="RetiredReason">废弃原因</param>
/// <param name="FinalizedAt">终结时间</param>
/// <param name="FinalizedBy">终结操作人</param>
/// <param name="DeclaredByBuilds">声明该订阅的构建，仅统计存活实例</param>
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
