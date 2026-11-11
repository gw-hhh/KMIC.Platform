namespace Kmic.BuildingBlocks.Application.Messaging;

/// <summary>
/// 消息发送
/// </summary>
/// <remarks>
/// 给端点和后台入口用，业务代码不直接用 Mediator 的类型
/// </remarks>
public interface ISender : ICommandSender, IQuerySender;
