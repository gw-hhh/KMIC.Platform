namespace Kmic.BuildingBlocks.Contracts.Messaging;

/// <summary>
/// 请求型消息：一对一地请求平台模块做一件事，如发送通知
/// </summary>
/// <remarks>
/// 只能定义在平台模块的 Contracts 中，接收方必须且只能是定义它的模块；业务模块之间协作只用事件（启动校验）
/// </remarks>
public interface IIntegrationRequest : IIntegrationMessage;
