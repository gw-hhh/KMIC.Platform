namespace Kmic.BuildingBlocks.Contracts.Messaging;

/// <summary>集成请求消息（如发送通知）；仅限平台基础模块定义，业务模块之间统一使用集成事件。</summary>
public interface IIntegrationRequest : IIntegrationMessage;
