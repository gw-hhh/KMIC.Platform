namespace Kmic.BuildingBlocks.Infrastructure.Auditing;

/// <summary>审计配置</summary>
public sealed class AuditOptions
{
    public const string Section = "Kmic:Audit";

    /// <summary>审计记录单个字段值的最大长度，超出后截断</summary>
    public int MaxValueLength { get; set; } = 1000;
}
