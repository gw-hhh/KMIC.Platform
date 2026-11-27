using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Contracts.Messaging;

namespace Kmic.BuildingBlocks.Application.Integration;

public interface IMessagingBuilder
{
    /// <summary>订阅集成消息，接收后映射为本模块命令并以系统身份执行；订阅名会持久化至收件箱，上线后不可更改。</summary>
    void Subscribe<TMessage, TCommand>(string name, Func<TMessage, MessageEnvelope, TCommand> map, Action<SubscriptionOptions>? configure = null)
        where TMessage : class, IIntegrationMessage
        where TCommand : ICommand;

    /// <summary>声明订阅已停用并标记为待终结；处理逻辑可移除，但本声明须保留至路由终结完成。</summary>
    void Retired(string name, string reason);
}
