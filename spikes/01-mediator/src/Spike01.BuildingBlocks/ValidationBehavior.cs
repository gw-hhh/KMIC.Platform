using Mediator;

namespace Spike01.BuildingBlocks;

/// <summary>
/// 管道第 3 位：全部消息；失败时用 IResultFactory 构造失败结果
/// </summary>
public sealed class ValidationBehavior<TMessage, TResponse>(PipelineTrace trace) : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
    where TResponse : Result, IResultFactory<TResponse>
{
    public ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        trace.Add($"Validation:{typeof(TMessage).Name}");
        if (message is IValidatable validatable && validatable.Validate() is { } error)
        {
            return ValueTask.FromResult(TResponse.Fail(error));
        }

        return next(message, cancellationToken);
    }
}
