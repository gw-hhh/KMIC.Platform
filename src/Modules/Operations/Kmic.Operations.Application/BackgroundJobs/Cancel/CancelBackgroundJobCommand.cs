using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.BackgroundJobs.Cancel;

/// <summary>
/// 请求取消任意用户的后台作业
/// </summary>
/// <remarks>
/// 运行中的作业要响应取消信号才会停，已提交的批次不回滚；提交人取消自己的作业走 my-jobs 端点，不需要管理作业权限
/// </remarks>
/// <param name="Module">作业所在的模块</param>
/// <param name="JobId">作业 ID</param>
[RequirePermission(OperationsPermissions.JobsManage)]
[Audited]
[Idempotent]
public sealed record CancelBackgroundJobCommand(string Module, Guid JobId) : ICommand;
