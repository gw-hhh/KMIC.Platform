using Mediator;

namespace Spike01.BuildingBlocks;

/// <summary>
/// 管道第 1 位：全部消息
/// </summary>
public sealed class LoggingBehavior<TMessage, TResponse>(PipelineTrace trace) : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        trace.Add($"Logging:{typeof(TMessage).Name}");
        return next(message, cancellationToken);
    }
}
