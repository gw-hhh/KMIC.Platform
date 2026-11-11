namespace Kmic.BuildingBlocks.Infrastructure.Auditing;

/// <summary>
/// 审计配置
/// </summary>
public sealed class AuditOptions
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public const string Section = "Kmic:Audit";

    /// <summary>
    /// 审计记录里单个字段值的最大长度，超过的截断
    /// </summary>
    public int MaxValueLength { get; set; } = 1000;
}
