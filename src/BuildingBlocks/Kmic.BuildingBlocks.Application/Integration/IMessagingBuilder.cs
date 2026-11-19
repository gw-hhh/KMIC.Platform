using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Contracts.Messaging;

namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 模块的订阅声明
/// </summary>
public interface IMessagingBuilder
{
    /// <summary>
    /// 订阅一种消息，收到后转成本模块的命令以系统身份执行
    /// </summary>
    /// <remarks>
    /// 订阅名存进 Inbox，发布后不能改
    /// </remarks>
    void Subscribe<TMessage, TCommand>(string name, Func<TMessage, MessageEnvelope, TCommand> map, Action<SubscriptionOptions>? configure = null)
        where TMessage : class, IIntegrationMessage
        where TCommand : ICommand;

    /// <summary>
    /// 声明订阅不再使用，转为待终结
    /// </summary>
    /// <remarks>
    /// 处理代码可以删，这条声明要留着
    /// </remarks>
    void Retired(string name, string reason);
}
