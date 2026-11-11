namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 本实例代码里声明退役的订阅
/// </summary>
/// <param name="Subscription">订阅名</param>
/// <param name="Reason">原因</param>
internal sealed record RouteRetirement(string Subscription, string Reason);
