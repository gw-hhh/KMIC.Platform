namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

// 代码中声明的订阅路由（NeedsInitialization 表示首次登记需重建读模型）
internal sealed record RouteDeclaration(MessageRoute Route, bool NeedsInitialization);
