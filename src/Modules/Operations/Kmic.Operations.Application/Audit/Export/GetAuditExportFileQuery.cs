using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Contracts.Files;

namespace Kmic.Operations.Application.Audit.Export;

/// <summary>获取审计导出文件下载信息。</summary>
[RequirePermission(OperationsPermissions.AuditExport)]
[AuditedRead]
public sealed record GetAuditExportFileQuery(Guid JobId) : IQuery<FileDownload>;
