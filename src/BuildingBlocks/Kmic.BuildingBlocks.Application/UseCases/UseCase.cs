namespace Kmic.BuildingBlocks.Application.UseCases;

public sealed record UseCase
{
    public required Type MessageType { get; init; }

    public required string Module { get; init; }

    public required MessageKind Kind { get; init; }

    public required AccessRequirement Requirement { get; init; }

    public string? Permission { get; init; }

    public bool AllowSystem { get; init; }

    public bool AllowWhenPasswordChangeRequired { get; init; }

    /// <summary>只读模拟会话中是否允许执行。</summary>
    public bool AllowWhenImpersonationReadOnly { get; init; }

    /// <summary>消息容量软线熔断期间是否仍允许执行。</summary>
    public bool ExemptFromMessagingSuspension { get; init; }

    /// <summary>契约查询授权方式，仅限 Contracts 查询。</summary>
    public ContractAccess? ContractAccess { get; init; }

    public bool Idempotent { get; init; }

    public bool? Audited { get; init; }

    public string? AuditAction { get; init; }

    public bool AuditedRead { get; init; }

    public string? FeatureGate { get; init; }

    public string? BypassesDataScope { get; init; }

    /// <summary>完整用例名称，如 Sales.SubmitQuotationCommand。</summary>
    public string Name => $"{Module}.{MessageType.Name}";
}
