using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.BackgroundJobs.Cancel;

/// <summary>
/// 管理员请求取消一个后台作业
/// </summary>
/// <remarks>
/// 排队中的直接取消，运行中的改为“取消中”，执行器在下次续期时停下。取消不是回滚，已提交的批次保留。
/// 提交人自己取消走各模块路由组下的作业端点，不经这里
/// </remarks>
/// <param name="Module">作业所在的模块</param>
/// <param name="JobId">作业 ID</param>
[RequirePermission(OperationsPermissions.JobsManage)]
[Audited]
[Idempotent]
public sealed record CancelBackgroundJobCommand(string Module, Guid JobId) : ICommand;
