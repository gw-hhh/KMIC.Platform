using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Operations.Application.Audit.List;

/// <summary>
/// 查询审计记录
/// </summary>
/// <remarks>
/// 时间范围必填且不超过 93 天；按发生时间倒序，游标分页，不数总数。审计查询不套数据范围，
/// 有权限点的审计人员看到全部；这个查询本身也会留下读取审计
/// </remarks>
/// <param name="From">起始时间，含</param>
/// <param name="To">结束时间，不含</param>
/// <param name="ActorUserId">操作人</param>
/// <param name="ActualUserId">实际操作人：模拟登录时真正动手的人</param>
/// <param name="Module">产生审计的模块</param>
/// <param name="Action">操作名</param>
/// <param name="ResourceType">操作对象的类型</param>
/// <param name="ResourceId">操作对象的 ID；不给类型只给 ID 也可以</param>
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
