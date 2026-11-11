using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Jobs;

/// <summary>
/// 提交后台作业
/// </summary>
/// <remarks>
/// 只能在命令里调用，作业要属于命令所在的模块。作业和业务数据在同一个事务里提交，命令失败就没有这个作业。
/// 提交人、当前角色取自执行上下文
/// </remarks>
public interface IBackgroundJobs
{
    /// <summary>
    /// 排队一个作业，返回作业 ID；同一用户这种作业排队和运行中的数量到上限时返回 Common.TooManyJobs
    /// </summary>
    Task<Result<Guid>> EnqueueAsync(IBackgroundJob job, CancellationToken ct);
}
