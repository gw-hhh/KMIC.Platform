namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// 用例目录中的一项：一个消息的全部横切声明，行为在运行时只查它，不再反射
/// </summary>
public sealed record UseCase
{
    /// <summary>
    /// 消息类型
    /// </summary>
    public required Type MessageType { get; init; }

    /// <summary>
    /// 所属模块，取自程序集名 Kmic.{模块}.Application
    /// </summary>
    public required string Module { get; init; }

    /// <summary>
    /// 命令或查询
    /// </summary>
    public required MessageKind Kind { get; init; }

    /// <summary>
    /// 授权声明种类
    /// </summary>
    public required AccessRequirement Requirement { get; init; }

    /// <summary>
    /// 需要的权限点；只有 Requirement 为 Permission 时有值
    /// </summary>
    public string? Permission { get; init; }

    /// <summary>
    /// 是否也允许系统身份执行
    /// </summary>
    public bool AllowSystem { get; init; }

    /// <summary>
    /// 必须先改密码时是否仍可执行
    /// </summary>
    public bool AllowWhenPasswordChangeRequired { get; init; }

    /// <summary>
    /// Contracts 查询的授权语义；不是 Contracts 查询时为空
    /// </summary>
    public ContractAccess? ContractAccess { get; init; }

    /// <summary>
    /// 是否需要幂等键
    /// </summary>
    public bool Idempotent { get; init; }

    /// <summary>
    /// 命令是否写审计事件；查询为空
    /// </summary>
    public bool? Audited { get; init; }

    /// <summary>
    /// 审计动作名，默认为“模块.命令名去掉 Command 后缀”
    /// </summary>
    public string? AuditAction { get; init; }

    /// <summary>
    /// 查询是否写读取审计
    /// </summary>
    public bool AuditedRead { get; init; }

    /// <summary>
    /// 处理器绕过数据范围的原因；不绕过时为空
    /// </summary>
    public string? BypassesDataScope { get; init; }

    /// <summary>
    /// 消息名，用于日志与追踪：“模块.类名”
    /// </summary>
    public string Name => $"{Module}.{MessageType.Name}";
}
