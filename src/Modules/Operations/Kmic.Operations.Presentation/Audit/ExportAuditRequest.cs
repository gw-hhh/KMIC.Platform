using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Operations.Presentation.Audit;

/// <summary>
/// 提交审计导出的请求
/// </summary>
/// <param name="From">起始时间，含；必填</param>
/// <param name="To">结束时间，不含；必填</param>
/// <param name="ActorUserId">操作人</param>
/// <param name="ActualUserId">实际操作人：模拟登录时真正动手的人</param>
/// <param name="Module">产生审计的模块，如 Identity</param>
/// <param name="Action">操作名，如 Identity.CreateUser</param>
/// <param name="ResourceType">操作对象的类型，如 User</param>
/// <param name="ResourceId">操作对象的 ID</param>
public sealed record ExportAuditRequest(
    DateTimeOffset From,
    DateTimeOffset To,
    UserId? ActorUserId = null,
    UserId? ActualUserId = null,
    string? Module = null,
    string? Action = null,
    string? ResourceType = null,
    Guid? ResourceId = null);
