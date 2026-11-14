using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.BackgroundJobs.Mine;

/// <summary>
/// 提交人取消自己提交的后台作业
/// </summary>
/// <remarks>
/// 排队中的直接取消，运行中的改为“取消中”，执行器在下次续期时停下。取消不是回滚，已提交的批次保留。
/// 别人提交的作业和不存在的一样返回“不存在”；管理员取消别人的作业用 <c>CancelBackgroundJobCommand</c>
/// </remarks>
/// <param name="Module">作业所在的模块</param>
/// <param name="JobId">作业 ID</param>
[AllowAuthenticated]
[Audited]
[Idempotent]
public sealed record CancelMyJobCommand(string Module, Guid JobId) : ICommand;
