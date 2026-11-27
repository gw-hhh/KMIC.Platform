using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.BackgroundJobs.Mine;

/// <summary>查询当前用户在指定模块提交的后台作业。</summary>
/// <param name="Module">模块</param>
/// <param name="JobId">作业 ID</param>
[AllowAuthenticated]
public sealed record GetMyJobQuery(string Module, Guid JobId) : IQuery<BackgroundJobItem>;
