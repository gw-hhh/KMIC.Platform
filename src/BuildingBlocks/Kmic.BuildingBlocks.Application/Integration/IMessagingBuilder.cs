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
    /// 订阅名存进 Inbox，发布后不能改。命令要标 [SystemOnly] 或 [AllowSystem]
    /// </remarks>
    /// <param name="name">订阅名，以本模块名开头，如 Quotation.SyncCustomerSummary</param>
    /// <param name="map">把消息转成命令</param>
    /// <param name="configure">有序处理、读模型的重建方式</param>
    void Subscribe<TMessage, TCommand>(string name, Func<TMessage, MessageEnvelope, TCommand> map, Action<SubscriptionOptions>? configure = null)
        where TMessage : class, IIntegrationMessage
        where TCommand : ICommand;

    /// <summary>
    /// 声明一个订阅不再使用
    /// </summary>
    /// <remarks>
    /// 订阅改为待终结：消息照常投递并积压，等运维确认可以终结。处理代码可以删掉，这条声明要留着
    /// </remarks>
    void Retired(string name, string reason);
}
