using System.ComponentModel.DataAnnotations;

namespace Kmic.Operations.Application.Audit.Export;

/// <summary>审计导出配置。</summary>
public sealed class AuditExportOptions
{
    public const string Section = "Operations:Audit";

    /// <summary>导出文件保留天数（到期且未关联时可清理）。</summary>
    [Range(1, 30)]
    public int ExportRetentionDays { get; init; } = 7;

    public TimeSpan ExportRetention => TimeSpan.FromDays(ExportRetentionDays);
}
