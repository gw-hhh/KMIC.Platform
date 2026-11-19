namespace Kmic.BuildingBlocks.Application.Messaging;

/// <summary>
/// 消息发送
/// </summary>
public interface ISender : ICommandSender, IQuerySender;
