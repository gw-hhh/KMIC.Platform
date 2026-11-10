namespace Kmic.BuildingBlocks.Application.Events;

/// <summary>
/// 登记只在提交成功后执行的动作
/// </summary>
/// <remarks>
/// 只用于失败也无妨的动作，目前只有缓存失效；必须可靠送达的一律走 Outbox（05-02 D6）
/// </remarks>
public interface IAfterCommit
{
    /// <summary>
    /// 登记动作；执行失败只记警告，不影响已返回的结果
    /// </summary>
    void Enqueue(Func<CancellationToken, ValueTask> action);
}
