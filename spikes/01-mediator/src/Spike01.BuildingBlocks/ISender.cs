namespace Spike01.BuildingBlocks;

/// <summary>
/// 端点与后台入口使用的发送入口
/// </summary>
public interface ISender : ICommandSender, IQuerySender;
