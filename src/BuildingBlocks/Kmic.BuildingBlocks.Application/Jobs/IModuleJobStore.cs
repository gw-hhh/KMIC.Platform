using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Jobs;

/// <summary>
/// 一个模块的后台作业存储：只读汇总和请求取消，给 Operations 的管理视图用
/// </summary>
/// <remarks>
/// 作业行在各自模块的框架表里（和业务数据同一个事务提交），Operations 通过每个模块的这个存储遍历。
/// 这与投递程序一样是框架基础设施的例外，不是 Operations 读取别的模块的业务数据：存储只暴露作业的状态，
/// 看不到作业参数与检查点（参数里可能带筛选条件之类的业务内容）
/// </remarks>
public interface IModuleJobStore
{
    /// <summary>
    /// 模块名
    /// </summary>
    string Module { get; }

    /// <summary>
    /// 按提交时间倒序取一批，时间相同按 ID 倒序
    /// </summary>
    /// <param name="filter">筛选条件</param>
    /// <param name="after">从这个位置之后取（不含）；第一批不给</param>
    /// <param name="take">最多取几条</param>
    /// <param name="ct">取消令牌</param>
    Task<IReadOnlyList<JobView>> ListAsync(JobFilter filter, JobPosition? after, int take, CancellationToken ct);

    /// <summary>
    /// 按 ID 取一个作业，没有时返回空
    /// </summary>
    /// <param name="id">作业 ID</param>
    /// <param name="ct">取消令牌</param>
    Task<JobView?> FindAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// 请求取消：排队中的直接取消，运行中的改为取消中，执行器在下次续期时停下。取消不是回滚，已提交的批次保留
    /// </summary>
    /// <param name="id">作业 ID</param>
    /// <param name="by">谁请求的</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>作业已经结束（或不存在）时返回 false</returns>
    Task<bool> RequestCancelAsync(Guid id, UserId by, CancellationToken ct);
}
