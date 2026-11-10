namespace Kmic.BuildingBlocks.Application.Messaging;

/// <summary>
/// 端点与后台入口使用的发送入口；业务代码不直接使用 Mediator 的类型
/// </summary>
public interface ISender : ICommandSender, IQuerySender;
