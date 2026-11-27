namespace Kmic.Operations.Application.Audit.Export;

/// <summary>审计导出作业检查点。</summary>
/// <param name="Registered">文件是否已发起登记</param>
public sealed record AuditExportCheckpoint(bool Registered);
