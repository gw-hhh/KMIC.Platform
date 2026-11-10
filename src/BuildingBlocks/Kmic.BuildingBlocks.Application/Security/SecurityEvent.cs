namespace Kmic.BuildingBlocks.Application.Security;

/// <summary>
/// 一条安全事件；身份、会话、IP、追踪 ID 与时间由写入器从执行上下文补全
/// </summary>
/// <param name="Type">事件类型，如 AuthorizationDenied、LoginFailed</param>
/// <param name="Severity">严重程度</param>
/// <param name="Reason">原因编码，如 MissingPermission</param>
public sealed record SecurityEvent(string Type, SecuritySeverity Severity, string? Reason)
{
    /// <summary>
    /// 相关用户；为空时由写入器取当前用户
    /// </summary>
    public Guid? UserId { get; init; }

    /// <summary>
    /// 登录失败时用户输入的账号（截断 100 字符），用于识别撞库
    /// </summary>
    public string? AttemptedAccount { get; init; }

    /// <summary>
    /// 相关资源类型
    /// </summary>
    public string? ResourceType { get; init; }

    /// <summary>
    /// 相关资源 ID
    /// </summary>
    public Guid? ResourceId { get; init; }

    /// <summary>
    /// 补充说明
    /// </summary>
    public IReadOnlyDictionary<string, string>? Details { get; init; }
}
