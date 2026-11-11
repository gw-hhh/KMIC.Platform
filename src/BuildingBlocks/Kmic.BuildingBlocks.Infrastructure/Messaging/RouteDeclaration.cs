namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 本实例代码里声明的一条路由
/// </summary>
/// <param name="Route">路由</param>
/// <param name="NeedsInitialization">第一次登记时要不要从源模块重建读模型</param>
internal sealed record RouteDeclaration(MessageRoute Route, bool NeedsInitialization);
