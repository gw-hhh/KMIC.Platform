using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Contracts.Files;

namespace Kmic.Operations.Application.Audit.Export;

/// <summary>
/// 取审计导出的结果文件
/// </summary>
/// <remarks>
/// 只有提交导出的人和有管理作业权限的人能取，别人看到的和不存在的一样。作业没做完返回可重试的 409，
/// 文件已经写好但还没登记完时也是 409。每次下载都留读取审计
/// </remarks>
/// <param name="JobId">导出作业</param>
[RequirePermission(OperationsPermissions.AuditExport)]
[AuditedRead]
public sealed record GetAuditExportFileQuery(Guid JobId) : IQuery<FileDownload>;
