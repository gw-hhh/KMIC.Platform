using Kmic.BuildingBlocks.Application.Messaging;

namespace Kmic.BuildingBlocks.Application.Integration;

internal sealed record SubscriptionDefinition(
    string Name,
    string Module,
    IntegrationMessageType Message,
    Type CommandType,
    Func<object, MessageEnvelope, ICommand> Map,
    bool IsOrdered,
    IReadModelRebuild? Rebuild);
