using Kmic.BuildingBlocks.Application.Integration;

namespace Kmic.BuildingBlocks.Testing.Messaging;

/// <summary>已写入发件箱的消息记录。</summary>
/// <param name="Message">消息体</param>
/// <param name="Envelope">消息信封</param>
/// <param name="CausationId">触发该消息的上游消息 ID</param>
public sealed record PublishedMessage<T>(T Message, MessageEnvelope Envelope, Guid? CausationId);
