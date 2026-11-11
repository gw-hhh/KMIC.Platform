using Kmic.BuildingBlocks.Application.Integration;

namespace Kmic.BuildingBlocks.Testing.Messaging;

/// <summary>
/// 写进 Outbox 的一条消息
/// </summary>
/// <param name="Message">消息</param>
/// <param name="Envelope">信封</param>
/// <param name="CausationId">引发它的消息 ID，在处理别的消息时发布才有</param>
public sealed record PublishedMessage<T>(T Message, MessageEnvelope Envelope, Guid? CausationId);
