namespace Kmic.Operations.Application.Messages.Routes;

/// <summary>
/// 框架订阅路由的只读查询结果
/// </summary>
/// <remarks>
/// 查询将状态转换为 <c>MessageRouteStatus</c> 编码
/// </remarks>
public sealed record MessageRouteRow
{
    /// <summary>
    /// 消息名称
    /// </summary>
    public string MessageType { get; init; } = string.Empty;

    /// <summary>
    /// 订阅名
    /// </summary>
    public string Subscription { get; init; } = string.Empty;

    /// <summary>
    /// 订阅方模块
    /// </summary>
    public string Module { get; init; } = string.Empty;

    /// <summary>
    /// 状态编码
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// 第一次登记它的构建号
    /// </summary>
    public string RegisteredBuild { get; init; } = string.Empty;

    /// <summary>
    /// 第一次登记的时间
    /// </summary>
    public DateTimeOffset RegisteredAt { get; init; }

    /// <summary>
    /// 是否仍需初始化读模型
    /// </summary>
    public bool NeedsInitialization { get; init; }

    /// <summary>
    /// 显示名
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// 退役原因
    /// </summary>
    public string? RetiredReason { get; init; }

    /// <summary>
    /// 终结的时间
    /// </summary>
    public DateTimeOffset? FinalizedAt { get; init; }

    /// <summary>
    /// 终结的人
    /// </summary>
    public Guid? FinalizedBy { get; init; }
}
