using Mediator;

namespace Spike01.BuildingBlocks;

/// <summary>
/// 管道第 6 位：只用于命令（泛型约束限定为命令）
/// </summary>
public sealed class TransactionBehavior<TMessage, TResponse>(PipelineTrace trace) : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IBaseCommand
{
    public async ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        trace.Add($"Transaction:begin:{typeof(TMessage).Name}");
        var response = await next(message, cancellationToken);
        trace.Add($"Transaction:commit:{typeof(TMessage).Name}");
        return response;
    }
}
