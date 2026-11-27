namespace Kmic.BuildingBlocks.Application.Security;

public sealed record SecurityEvent(string Type, SecuritySeverity Severity, string? Reason)
{
    /// <summary>用户 ID，为空时取当前用户。</summary>
    public Guid? UserId { get; init; }

    /// <summary>登录失败时输入的账号。</summary>
    public string? AttemptedAccount { get; init; }

    public string? ResourceType { get; init; }

    public Guid? ResourceId { get; init; }

    public IReadOnlyDictionary<string, string>? Details { get; init; }
}
