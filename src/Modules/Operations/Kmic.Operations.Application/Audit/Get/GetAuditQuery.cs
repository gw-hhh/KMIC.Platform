using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Operations.Domain.Audit;

namespace Kmic.Operations.Application.Audit.Get;

/// <summary>
/// 一条审计记录的详情，含字段级变更
/// </summary>
/// <param name="Id">审计记录 ID</param>
[RequirePermission(OperationsPermissions.AuditView)]
[AuditedRead]
public sealed record GetAuditQuery(AuditLogId Id) : IQuery<AuditDetail>;
