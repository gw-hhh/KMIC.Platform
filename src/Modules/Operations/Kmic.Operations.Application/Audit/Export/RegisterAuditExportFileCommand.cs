using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Files.Contracts;

namespace Kmic.Operations.Application.Audit.Export;

/// <summary>
/// 请求 Files 登记审计导出写好的文件，只由导出作业发送
/// </summary>
[RequirePermission(OperationsPermissions.AuditExport)]
[Audited]
public sealed record RegisterAuditExportFileCommand(GeneratedFile File) : ICommand;
