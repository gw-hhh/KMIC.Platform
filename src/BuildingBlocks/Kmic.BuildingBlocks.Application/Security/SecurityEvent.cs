namespace Kmic.BuildingBlocks.Application.Security;

/// <summary>
/// 安全事件
/// </summary>
/// <param name="Type">事件类型，如 AuthorizationDenied、LoginFailed</param>
/// <param name="Severity">严重程度</param>
/// <param name="Reason">原因编码，如 MissingPermission</param>
public sealed record SecurityEvent(string Type, SecuritySeverity Severity, string? Reason)
{
    /// <summary>
    /// 用户，为空时取当前用户
    /// </summary>
    public Guid? UserId { get; init; }

    /// <summary>
    /// 登录失败时输入的账号
    /// </summary>
    public string? AttemptedAccount { get; init; }

    /// <summary>
    /// 资源类型
    /// </summary>
    public string? ResourceType { get; init; }

    /// <summary>
    /// 资源 ID
    /// </summary>
    public Guid? ResourceId { get; init; }

    /// <summary>
    /// 补充说明
    /// </summary>
    public IReadOnlyDictionary<string, string>? Details { get; init; }
}
