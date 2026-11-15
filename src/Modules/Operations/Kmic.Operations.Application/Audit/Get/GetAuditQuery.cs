using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Operations.Domain.Audit;

namespace Kmic.Operations.Application.Audit.Get;

/// <summary>
/// 查询审计详情，含字段变更
/// </summary>
[RequirePermission(OperationsPermissions.AuditView)]
[AuditedRead]
public sealed record GetAuditQuery(AuditLogId Id) : IQuery<AuditDetail>;
