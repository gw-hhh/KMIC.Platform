namespace Kmic.BuildingBlocks.Contracts.Messaging;

/// <summary>
/// 请求消息，如发送通知
/// </summary>
/// <remarks>
/// 只能定义在平台模块里，由定义它的模块处理。业务模块之间只用事件
/// </remarks>
public interface IIntegrationRequest : IIntegrationMessage;
