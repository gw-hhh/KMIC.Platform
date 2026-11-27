namespace Kmic.BuildingBlocks.Application.Integration;

public interface IReadModelRebuildAdmin
{
    /// <summary>查询各订阅方模块的重建轮次，按时间倒序且每个订阅仅保留最近若干轮。</summary>
    Task<IReadOnlyList<RebuildRunInfo>> ListAsync(CancellationToken ct);

    /// <summary>为指定订阅手动触发一轮重建。</summary>
    Task<RebuildAdminOutcome> TriggerAsync(string subscription, CancellationToken ct);

    /// <summary>确认完成已暂停的重建轮次。</summary>
    Task<RebuildAdminOutcome> ConfirmAsync(string subscription, CancellationToken ct);

    /// <summary>放弃已暂停的重建轮次并清除初始化标记。</summary>
    Task<RebuildAdminOutcome> AbandonAsync(string subscription, CancellationToken ct);
}
