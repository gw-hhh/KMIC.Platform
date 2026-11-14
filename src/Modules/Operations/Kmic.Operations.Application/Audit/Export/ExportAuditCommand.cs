using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Operations.Application.Audit.Export;

/// <summary>
/// 提交审计导出：把一段时间的审计记录导成 CSV 文件
/// </summary>
/// <remarks>
/// 做成后台作业，返回作业 ID。筛选条件和审计列表一样，导出的就是列表筛出来的全部；
/// 行数超过 <c>Kmic:Jobs:ExportMaxRows</c> 时作业失败，要缩小时间范围
/// </remarks>
/// <param name="From">起始时间，含</param>
/// <param name="To">结束时间，不含</param>
/// <param name="ActorUserId">操作人</param>
/// <param name="ActualUserId">实际操作人：模拟登录时真正动手的人</param>
/// <param name="Module">产生审计的模块</param>
/// <param name="Action">操作名</param>
/// <param name="ResourceType">操作对象的类型</param>
/// <param name="ResourceId">操作对象的 ID</param>
[RequirePermission(OperationsPermissions.AuditExport)]
[Audited]
[Idempotent]
public sealed record ExportAuditCommand(
    DateTimeOffset From,
    DateTimeOffset To,
    UserId? ActorUserId,
    UserId? ActualUserId,
    string? Module,
    string? Action,
    string? ResourceType,
    Guid? ResourceId) : ICommand<JobAccepted>;
