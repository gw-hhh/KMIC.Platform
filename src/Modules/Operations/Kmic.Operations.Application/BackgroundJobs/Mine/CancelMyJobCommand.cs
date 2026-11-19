using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.BackgroundJobs.Mine;

/// <summary>
/// 请求取消当前用户提交的后台作业
/// </summary>
/// <remarks>
/// 运行中的作业要响应取消信号才会停，已提交的批次不回滚；作业不存在或不是自己提交的都返回作业不存在
/// </remarks>
/// <param name="Module">作业所在的模块</param>
/// <param name="JobId">作业 ID</param>
[AllowAuthenticated]
[Audited]
[Idempotent]
public sealed record CancelMyJobCommand(string Module, Guid JobId) : ICommand;
