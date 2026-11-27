namespace Kmic.BuildingBlocks.Application.Events;

/// <summary>事务提交后回调，仅用于允许失败的轻量动作；需可靠送达的逻辑请使用 Outbox。</summary>
public interface IAfterCommit
{
    /// <summary>注册提交后动作，执行失败时仅记录警告日志。</summary>
    void Enqueue(Func<CancellationToken, ValueTask> action);

    /// <summary>提交后失效指定标签的缓存。</summary>
    void InvalidateTags(params IEnumerable<string> tags);
}
