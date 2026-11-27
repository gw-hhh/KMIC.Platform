using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>放弃死信前的守卫校验器，负责调度接收方注册的检查策略。</summary>
public interface IAbandonGuard
{
    /// <summary>校验指定收件箱死信当前是否允许放弃；未注册策略或校验通过时返回成功，反序列化失败或未注册消息返回失败。</summary>
    Task<Result> CheckAsync(DeadMessageDetail message, CancellationToken ct);
}
