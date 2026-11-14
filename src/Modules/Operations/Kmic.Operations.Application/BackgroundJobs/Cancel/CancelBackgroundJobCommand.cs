using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.BackgroundJobs.Cancel;

/// <summary>
/// 管理员请求取消后台作业
/// </summary>
/// <remarks>
/// 运行中的作业须响应取消信号才会停止；已提交的批次不回滚。
/// 提交人取消自己的作业使用 Operations 的 my-jobs 端点，无需管理员权限
/// </remarks>
/// <param name="Module">作业所在的模块</param>
/// <param name="JobId">作业 ID</param>
[RequirePermission(OperationsPermissions.JobsManage)]
[Audited]
[Idempotent]
public sealed record CancelBackgroundJobCommand(string Module, Guid JobId) : ICommand;
