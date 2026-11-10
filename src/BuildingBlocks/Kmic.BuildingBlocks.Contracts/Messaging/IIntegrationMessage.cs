namespace Kmic.BuildingBlocks.Contracts.Messaging;

/// <summary>
/// 经 Outbox / Inbox 在模块之间传递的消息；具体类型必须标注 IntegrationMessageAttribute
/// </summary>
public interface IIntegrationMessage;
