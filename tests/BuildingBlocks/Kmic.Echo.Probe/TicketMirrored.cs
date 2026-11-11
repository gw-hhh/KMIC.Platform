using Kmic.BuildingBlocks.Contracts.Messaging;

namespace Kmic.Echo.Probe;

[IntegrationMessage("Echo.TicketMirrored")]
internal sealed record TicketMirrored(Guid TicketId, string Title, long SourceVersion) : IIntegrationEvent;
