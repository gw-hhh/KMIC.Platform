using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.BackgroundJobs.List;

/// <summary>
/// 查询各模块的后台作业
/// </summary>
/// <remarks>
/// 遍历每个模块自己的作业表，按提交时间倒序归并，游标分页。看不到作业参数
/// </remarks>
/// <param name="Module">只看这个模块的；不给就是全部模块</param>
/// <param name="JobType">作业类型名</param>
/// <param name="Status">状态编码：Queued、Running、Succeeded、Failed、Cancelling、Cancelled</param>
/// <param name="SubmittedBy">提交人</param>
/// <param name="Cursor">游标分页参数</param>
[RequirePermission(OperationsPermissions.JobsView)]
public sealed record ListBackgroundJobsQuery(string? Module, string? JobType, string? Status, Guid? SubmittedBy, CursorRequest Cursor)
    : IQuery<CursorPage<BackgroundJobItem>>;
