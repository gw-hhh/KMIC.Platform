using System.ComponentModel;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Operations.Presentation.Audit;

/// <summary>
/// 查询审计记录的参数
/// </summary>
/// <param name="From">起始时间，含；必填</param>
/// <param name="To">结束时间，不含；必填，和起始时间相隔不超过 93 天</param>
/// <param name="ActorUserId">操作人</param>
/// <param name="ActualUserId">实际操作人：模拟登录时真正动手的人</param>
/// <param name="Module">产生审计的模块，如 Identity</param>
/// <param name="Action">操作名，如 Identity.CreateUser</param>
/// <param name="ResourceType">操作对象的类型，如 User</param>
/// <param name="ResourceId">操作对象的 ID</param>
/// <param name="After">上一页返回的游标；第一页不给</param>
/// <param name="Limit">每页数量，1 到 200，默认 50</param>
public sealed record ListAuditRequest(
    DateTimeOffset From,
    DateTimeOffset To,
    UserId? ActorUserId,
    UserId? ActualUserId,
    string? Module,
    string? Action,
    string? ResourceType,
    Guid? ResourceId,
    string? After,
    [property: DefaultValue(50)] int Limit = 50);
