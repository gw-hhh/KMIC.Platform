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
public sealed record ExportAuditRequest(
    DateTimeOffset From,
    DateTimeOffset To,
    UserId? ActorUserId = null,
    UserId? ActualUserId = null,
    string? Module = null,
    string? Action = null,
    string? ResourceType = null,
    Guid? ResourceId = null);
