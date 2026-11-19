using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.BackgroundJobs.Cancel;

/// <summary>
/// 请求取消任意用户的后台作业
/// </summary>
/// <remarks>
/// 已提交的批次不回滚
/// </remarks>
/// <param name="Module">作业所在的模块</param>
/// <param name="JobId">作业 ID</param>
[RequirePermission(OperationsPermissions.JobsManage)]
[Audited]
[Idempotent]
public sealed record CancelBackgroundJobCommand(string Module, Guid JobId) : ICommand;
