using System.ComponentModel;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Operations.Presentation.Audit;

/// <param name="From">起始时间</param>
/// <param name="To">结束时间</param>
/// <param name="ActorUserId">操作人 ID</param>
/// <param name="ActualUserId">实际操作人 ID（模拟身份时）</param>
/// <param name="Module">产生审计的模块</param>
/// <param name="Action">操作名</param>
/// <param name="ResourceType">操作对象类型</param>
/// <param name="ResourceId">操作对象 ID</param>
/// <param name="After">上一页返回的游标，首页不传</param>
/// <param name="Limit">每页数量</param>
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
