using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

[AllowAuthenticated]
internal sealed record GetTicketTitleQuery(TicketId Id) : IQuery<string>;
