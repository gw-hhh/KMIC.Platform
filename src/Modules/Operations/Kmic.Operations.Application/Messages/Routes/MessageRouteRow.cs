namespace Kmic.Operations.Application.Messages.Routes;

/// <summary>框架订阅路由只读查询结果</summary>
public sealed record MessageRouteRow
{
    public string MessageType { get; init; } = string.Empty;

    public string Subscription { get; init; } = string.Empty;

    public string Module { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public string RegisteredBuild { get; init; } = string.Empty;

    public DateTimeOffset RegisteredAt { get; init; }

    /// <summary>是否仍需初始化读模型</summary>
    public bool NeedsInitialization { get; init; }

    public string? DisplayName { get; init; }

    public string? RetiredReason { get; init; }

    public DateTimeOffset? FinalizedAt { get; init; }

    public Guid? FinalizedBy { get; init; }
}
