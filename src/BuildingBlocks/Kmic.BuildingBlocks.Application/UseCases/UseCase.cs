namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// 用例
/// </summary>
/// <remarks>
/// 一个消息的全部声明，管道运行时只查它，不再反射
/// </remarks>
public sealed record UseCase
{
    /// <summary>
    /// 消息类型
    /// </summary>
    public required Type MessageType { get; init; }

    /// <summary>
    /// 模块
    /// </summary>
    public required string Module { get; init; }

    /// <summary>
    /// 消息种类
    /// </summary>
    public required MessageKind Kind { get; init; }

    /// <summary>
    /// 授权声明类型
    /// </summary>
    public required AccessRequirement Requirement { get; init; }

    /// <summary>
    /// 权限点
    /// </summary>
    public string? Permission { get; init; }

    /// <summary>
    /// 是否允许系统身份执行
    /// </summary>
    public bool AllowSystem { get; init; }

    /// <summary>
    /// 需要改密码时是否仍可执行
    /// </summary>
    public bool AllowWhenPasswordChangeRequired { get; init; }

    /// <summary>
    /// 只读的模拟会话里是否仍可执行
    /// </summary>
    public bool AllowWhenImpersonationReadOnly { get; init; }

    /// <summary>
    /// Contracts 查询的授权方式
    /// </summary>
    public ContractAccess? ContractAccess { get; init; }

    /// <summary>
    /// 是否需要幂等键
    /// </summary>
    public bool Idempotent { get; init; }

    /// <summary>
    /// 是否记录审计
    /// </summary>
    public bool? Audited { get; init; }

    /// <summary>
    /// 审计操作名
    /// </summary>
    public string? AuditAction { get; init; }

    /// <summary>
    /// 是否记录读取审计
    /// </summary>
    public bool AuditedRead { get; init; }

    /// <summary>
    /// 功能开关名；没有声明时为空
    /// </summary>
    public string? FeatureGate { get; init; }

    /// <summary>
    /// 绕过数据权限的原因
    /// </summary>
    public string? BypassesDataScope { get; init; }

    /// <summary>
    /// 消息名，如 Sales.SubmitQuotationCommand
    /// </summary>
    public string Name => $"{Module}.{MessageType.Name}";
}
