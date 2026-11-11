using System.Collections.Concurrent;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Presentation.Tests.Support;

/// <summary>
/// 测试给每种消息预设的应答：接口层测试只关心 HTTP，不跑真正的处理器
/// </summary>
internal sealed class SenderScript
{
    private readonly ConcurrentDictionary<Type, Func<object, RequestMetadata, object>> _handlers = new();

    /// <summary>
    /// 命令或查询返回值的应答
    /// </summary>
    public void On<TMessage, TResponse>(Func<TMessage, RequestMetadata, Result<TResponse>> handler)
        where TMessage : notnull =>
        _handlers[typeof(TMessage)] = (message, metadata) => handler((TMessage)message, metadata);

    /// <summary>
    /// 不返回值的命令的应答
    /// </summary>
    public void On<TMessage>(Func<TMessage, RequestMetadata, Result> handler)
        where TMessage : notnull =>
        _handlers[typeof(TMessage)] = (message, metadata) => handler((TMessage)message, metadata);

    internal TResult Run<TResult>(object message, RequestMetadata metadata) =>
        _handlers.TryGetValue(message.GetType(), out var handler)
            ? (TResult)handler(message, metadata)
            : throw new InvalidOperationException($"测试没有给 {message.GetType().Name} 预设应答。");
}
