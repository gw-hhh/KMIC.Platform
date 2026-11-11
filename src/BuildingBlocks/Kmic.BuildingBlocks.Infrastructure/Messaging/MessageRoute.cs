namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 一条订阅路由
/// </summary>
/// <param name="MessageType">消息名称</param>
/// <param name="Subscription">订阅名</param>
/// <param name="Module">订阅方模块</param>
internal sealed record MessageRoute(string MessageType, string Subscription, string Module);
