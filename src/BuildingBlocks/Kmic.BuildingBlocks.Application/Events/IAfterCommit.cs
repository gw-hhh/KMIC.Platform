namespace Kmic.BuildingBlocks.Application.Events;

/// <summary>
/// 提交后执行的动作
/// </summary>
/// <remarks>
/// 只用于失败了也没关系的动作，比如清缓存。必须送达的走 Outbox
/// </remarks>
public interface IAfterCommit
{
    /// <summary>
    /// 登记动作，执行失败只记警告
    /// </summary>
    void Enqueue(Func<CancellationToken, ValueTask> action);

    /// <summary>
    /// 提交后清除这些标签的缓存
    /// </summary>
    /// <param name="tags">缓存标签</param>
    void InvalidateTags(params IEnumerable<string> tags);
}
