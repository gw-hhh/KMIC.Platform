using Kmic.BuildingBlocks.Contracts.Messaging;

namespace Kmic.BuildingBlocks.Application.Integration;

internal sealed record PendingMessage(IIntegrationMessage Message, IntegrationMessageType Type);
