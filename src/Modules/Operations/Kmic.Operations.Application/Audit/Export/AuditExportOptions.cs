using System.ComponentModel.DataAnnotations;

namespace Kmic.Operations.Application.Audit.Export;

/// <summary>
/// 审计导出的部署配置
/// </summary>
public sealed class AuditExportOptions
{
    /// <summary>
    /// 配置节
    /// </summary>
    public const string Section = "Operations:Audit";

    /// <summary>
    /// 导出的文件保留多少天，到期没有被关联就清理
    /// </summary>
    [Range(1, 30)]
    public int ExportRetentionDays { get; init; } = 7;

    /// <summary>
    /// 文件保留多久
    /// </summary>
    public TimeSpan ExportRetention => TimeSpan.FromDays(ExportRetentionDays);
}
