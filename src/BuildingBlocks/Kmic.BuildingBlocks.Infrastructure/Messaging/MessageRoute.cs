namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

internal sealed record MessageRoute(string MessageType, string Subscription, string Module);
