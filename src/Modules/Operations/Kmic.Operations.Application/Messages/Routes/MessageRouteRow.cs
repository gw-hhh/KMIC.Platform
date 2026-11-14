namespace Kmic.Operations.Application.Messages.Routes;

/// <summary>
/// 订阅路由表里的一行，只用来读：路由由框架在实例启动时登记
/// </summary>
/// <remarks>
/// 状态在查询里已经换成编码文字，见 <c>MessageRouteStatus</c>
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
    /// 是否还需要初始化：新订阅登记时要重建读模型
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
