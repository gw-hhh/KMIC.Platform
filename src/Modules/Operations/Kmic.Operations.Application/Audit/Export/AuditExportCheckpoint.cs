namespace Kmic.Operations.Application.Audit.Export;

/// <summary>
/// 审计导出作业的检查点：只有最后一步（登记文件）用到
/// </summary>
/// <param name="Registered">文件已经请求登记</param>
public sealed record AuditExportCheckpoint(bool Registered);
