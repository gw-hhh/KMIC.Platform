using Kmic.BuildingBlocks.Contracts.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 放弃死信前的检查，由接收方模块注册
/// </summary>
/// <remarks>
/// 放弃后会留下半截状态的消息要注册
/// </remarks>
/// <typeparam name="TMessage">被放弃的消息类型，通常是请求型消息</typeparam>
public interface IAbandonPolicy<in TMessage>
    where TMessage : IIntegrationMessage
{
    /// <summary>
    /// 这条消息现在能不能放弃
    /// </summary>
    /// <returns>成功表示可以放弃；失败的错误说明就是不能放弃的原因</returns>
    ValueTask<Result> CheckAsync(TMessage message, CancellationToken ct);
}
