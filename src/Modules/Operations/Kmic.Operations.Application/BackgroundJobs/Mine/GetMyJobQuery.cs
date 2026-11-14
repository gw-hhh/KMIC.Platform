using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.BackgroundJobs.Mine;

/// <summary>
/// 查询自己提交的一个后台作业
/// </summary>
/// <remarks>
/// 页面轮询进度用，登录就能看；别人提交的作业和不存在的一样返回“不存在”，不暴露它的存在
/// </remarks>
/// <param name="Module">作业所在的模块</param>
/// <param name="JobId">作业 ID</param>
[AllowAuthenticated]
public sealed record GetMyJobQuery(string Module, Guid JobId) : IQuery<BackgroundJobItem>;
