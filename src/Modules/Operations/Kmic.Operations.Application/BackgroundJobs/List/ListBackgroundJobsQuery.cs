using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.BackgroundJobs.List;

/// <summary>按提交时间倒序分页查询各模块后台作业（不含作业参数）。</summary>
/// <param name="Module">模块筛选</param>
/// <param name="JobType">作业类型名</param>
/// <param name="Status">状态编码：Queued、Running、Succeeded、Failed、Cancelling、Cancelled</param>
/// <param name="SubmittedBy">提交人</param>
/// <param name="Cursor">游标分页参数</param>
[RequirePermission(OperationsPermissions.JobsView)]
public sealed record ListBackgroundJobsQuery(string? Module, string? JobType, string? Status, Guid? SubmittedBy, CursorRequest Cursor)
    : IQuery<CursorPage<BackgroundJobItem>>;
