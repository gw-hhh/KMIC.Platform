namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 读模型重建的运维操作
/// </summary>
public interface IReadModelRebuildAdmin
{
    /// <summary>
    /// 各订阅方模块的重建轮次，新的在前，每个订阅最多列最近几轮
    /// </summary>
    Task<IReadOnlyList<RebuildRunInfo>> ListAsync(CancellationToken ct);

    /// <summary>
    /// 给订阅手动排一轮重建
    /// </summary>
    Task<RebuildAdminOutcome> TriggerAsync(string subscription, CancellationToken ct);

    /// <summary>
    /// 确认收尾：源端找不到的行太多而暂停的一轮，运维核实之后确认它们确实没有了，继续把它们置为终态并完成这一轮
    /// </summary>
    Task<RebuildAdminOutcome> ConfirmAsync(string subscription, CancellationToken ct);

    /// <summary>
    /// 放弃暂停的一轮，并清除订阅的“需要初始化”标记：此后不会自动重建，要重建由运维手动触发
    /// </summary>
    Task<RebuildAdminOutcome> AbandonAsync(string subscription, CancellationToken ct);
}
