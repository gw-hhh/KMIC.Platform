namespace Kmic.Operations.Application.Audit.Export;

/// <summary>审计导出结果摘要。</summary>
/// <param name="Rows">导出记录数</param>
/// <param name="FileName">文件名</param>
public sealed record AuditExportSummary(long Rows, string FileName);
