using Kmic.BuildingBlocks.Contracts.Messaging;

namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 等待写入 Outbox 的消息
/// </summary>
/// <param name="Message">消息</param>
/// <param name="Type">消息的声明</param>
internal sealed record PendingMessage(IIntegrationMessage Message, IntegrationMessageType Type);
