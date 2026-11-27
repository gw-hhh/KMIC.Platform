using Kmic.BuildingBlocks.Contracts.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>放弃死信前的前置检查策略（由接收方模块注册）；若放弃消息会遗留中间状态，须实现并注册本策略。</summary>
public interface IAbandonPolicy<in TMessage>
    where TMessage : IIntegrationMessage
{
    /// <summary>判断消息当前是否允许放弃；失败结果中的错误说明不可放弃的原因。</summary>
    ValueTask<Result> CheckAsync(TMessage message, CancellationToken ct);
}
