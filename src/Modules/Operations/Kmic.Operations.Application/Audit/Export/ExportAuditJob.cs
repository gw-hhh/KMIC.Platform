using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Operations.Application.Audit.Export;

/// <summary>
/// 审计导出作业的参数：和 <see cref="ExportAuditCommand"/> 一样的筛选条件
/// </summary>
/// <remarks>
/// 不改审计数据，中断后从头重做；重做可能多写一个存储对象，没登记的由对账清理
/// </remarks>
/// <param name="From">起始时间，含</param>
/// <param name="To">结束时间，不含</param>
/// <param name="ActorUserId">操作人</param>
/// <param name="ActualUserId">实际操作人</param>
/// <param name="Module">产生审计的模块</param>
/// <param name="Action">操作名</param>
/// <param name="ResourceType">操作对象的类型</param>
/// <param name="ResourceId">操作对象的 ID</param>
[BackgroundJob(Name, "审计导出", Permission = OperationsPermissions.AuditExport, Audited = true, Resumable = true)]
public sealed record ExportAuditJob(
    DateTimeOffset From,
    DateTimeOffset To,
    UserId? ActorUserId,
    UserId? ActualUserId,
    string? Module,
    string? Action,
    string? ResourceType,
    Guid? ResourceId) : IBackgroundJob
{
    /// <summary>
    /// 作业类型名
    /// </summary>
    public const string Name = "Operations.ExportAudit";
}
