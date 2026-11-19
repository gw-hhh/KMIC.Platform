using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Operations.Application.Audit.List;

/// <summary>
/// 查询审计记录
/// </summary>
/// <param name="From">起始时间</param>
/// <param name="To">结束时间</param>
/// <param name="ActorUserId">操作人 ID</param>
/// <param name="ActualUserId">模拟身份时的实际操作人 ID</param>
/// <param name="Module">产生审计的模块</param>
/// <param name="Action">操作名</param>
/// <param name="ResourceType">操作对象的类型</param>
/// <param name="ResourceId">操作对象 ID</param>
/// <param name="Cursor">游标分页参数</param>
[RequirePermission(OperationsPermissions.AuditView)]
[AuditedRead]
public sealed record ListAuditQuery(
    DateTimeOffset From,
    DateTimeOffset To,
    UserId? ActorUserId,
    UserId? ActualUserId,
    string? Module,
    string? Action,
    string? ResourceType,
    Guid? ResourceId,
    CursorRequest Cursor) : IQuery<CursorPage<AuditListItem>>;
