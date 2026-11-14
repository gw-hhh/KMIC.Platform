namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 读模型重建的运维操作：看各轮的状态、手动触发、确认收尾、放弃
/// </summary>
/// <remarks>
/// 给 Operations 的管理命令用。轮次和作业在订阅方模块的框架表里，由框架在自己的事务里读写，
/// 和重试、放弃死信一样是基础设施的写入，不在命令的事务里。订阅要在本实例的代码里声明了重建方式才能操作
/// </remarks>
public interface IReadModelRebuildAdmin
{
    /// <summary>
    /// 各订阅方模块的重建轮次，新的在前，每个订阅最多列最近几轮
    /// </summary>
    /// <param name="ct">取消令牌</param>
    Task<IReadOnlyList<RebuildRunInfo>> ListAsync(CancellationToken ct);

    /// <summary>
    /// 给订阅手动排一轮重建
    /// </summary>
    /// <param name="subscription">订阅名</param>
    /// <param name="ct">取消令牌</param>
    Task<RebuildAdminOutcome> TriggerAsync(string subscription, CancellationToken ct);

    /// <summary>
    /// 确认收尾：源端找不到的行太多而暂停的一轮，运维核实之后确认它们确实没有了，继续把它们置为终态并完成这一轮
    /// </summary>
    /// <param name="subscription">订阅名</param>
    /// <param name="ct">取消令牌</param>
    Task<RebuildAdminOutcome> ConfirmAsync(string subscription, CancellationToken ct);

    /// <summary>
    /// 放弃暂停的一轮，并清除订阅的“需要初始化”标记：此后不会自动重建，要重建由运维手动触发
    /// </summary>
    /// <param name="subscription">订阅名</param>
    /// <param name="ct">取消令牌</param>
    Task<RebuildAdminOutcome> AbandonAsync(string subscription, CancellationToken ct);
}
