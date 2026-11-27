using System.Collections.Concurrent;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Presentation.Tests.Support;

// 为接口层测试预设各类消息的模拟应答，避免执行真实处理器
internal sealed class SenderScript
{
    private readonly ConcurrentDictionary<Type, Func<object, RequestMetadata, object>> _handlers = new();

    public void On<TMessage, TResponse>(Func<TMessage, RequestMetadata, Result<TResponse>> handler)
        where TMessage : notnull =>
        _handlers[typeof(TMessage)] = (message, metadata) => handler((TMessage)message, metadata);

    public void On<TMessage>(Func<TMessage, RequestMetadata, Result> handler)
        where TMessage : notnull =>
        _handlers[typeof(TMessage)] = (message, metadata) => handler((TMessage)message, metadata);

    internal TResult Run<TResult>(object message, RequestMetadata metadata) =>
        _handlers.TryGetValue(message.GetType(), out var handler)
            ? (TResult)handler(message, metadata)
            : throw new InvalidOperationException($"测试没有给 {message.GetType().Name} 预设应答。");
}
