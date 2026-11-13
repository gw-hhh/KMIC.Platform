using Kmic.BuildingBlocks.Contracts.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 放弃一条死信之前的检查：由接收方模块为自己的消息注册
/// </summary>
/// <remarks>
/// 放弃意味着这条消息永远不会被处理。有些消息放弃了会留下半截状态：比如 Files 的 AttachFiles，
/// 引用的文件还处于“可用”或“已解除”就放弃，文件会永远停在没有归属的状态，所以这时只能重试。
/// 检查在接收方模块的作用域里执行（系统身份），可以查接收方自己的数据；返回失败就不改状态、把原因告诉运维。
/// 没有注册检查的消息可以直接放弃
/// </remarks>
/// <typeparam name="TMessage">被放弃的消息类型，通常是请求型消息</typeparam>
public interface IAbandonPolicy<in TMessage>
    where TMessage : IIntegrationMessage
{
    /// <summary>
    /// 这条消息现在能不能放弃
    /// </summary>
    /// <param name="message">载荷还原出的消息</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>成功表示可以放弃；失败的错误说明就是不能放弃的原因</returns>
    ValueTask<Result> CheckAsync(TMessage message, CancellationToken ct);
}
