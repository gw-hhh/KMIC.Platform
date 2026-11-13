using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.BackgroundJobs.Mine;

/// <summary>
/// 查询当前用户在各模块提交的后台作业
/// </summary>
/// <remarks>
/// 外壳的“作业入口”用它：登录就能看，只看得到自己提交的。模拟登录时看的是被模拟用户的作业
/// </remarks>
/// <param name="Status">状态编码：Queued、Running、Succeeded、Failed、Cancelling、Cancelled</param>
/// <param name="Cursor">游标分页参数</param>
[AllowAuthenticated]
public sealed record ListMyJobsQuery(string? Status, CursorRequest Cursor) : IQuery<CursorPage<BackgroundJobItem>>;
