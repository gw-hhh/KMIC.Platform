using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.ScheduledJobs.List;

/// <summary>
/// 查询全部定时任务
/// </summary>
/// <remarks>
/// 模块的和框架自带的都在里面，按模块、名称排好；代码里已经删除、覆盖设置还留着的任务也列出来
/// </remarks>
[RequirePermission(OperationsPermissions.JobsView)]
public sealed record ListScheduledJobsQuery : IQuery<IReadOnlyList<ScheduledJobItem>>;
