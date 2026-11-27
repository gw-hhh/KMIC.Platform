using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Files.Contracts;

namespace Kmic.Operations.Application.Audit.Export;

/// <summary>请求 Files 模块登记已生成的审计导出文件（仅由导出作业发送）。</summary>
[RequirePermission(OperationsPermissions.AuditExport)]
[Audited]
public sealed record RegisterAuditExportFileCommand(GeneratedFile File) : ICommand;
