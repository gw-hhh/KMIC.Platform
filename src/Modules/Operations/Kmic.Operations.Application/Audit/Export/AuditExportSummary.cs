namespace Kmic.Operations.Application.Audit.Export;

/// <summary>
/// 审计导出的结果摘要，存在作业记录里
/// </summary>
/// <param name="Rows">导出了多少条</param>
/// <param name="FileName">文件名</param>
public sealed record AuditExportSummary(long Rows, string FileName);
