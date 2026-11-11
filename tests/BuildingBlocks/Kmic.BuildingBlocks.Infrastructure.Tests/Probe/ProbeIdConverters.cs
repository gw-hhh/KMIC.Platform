using Vogen;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

[EfCoreConverter<TicketId>]
[EfCoreConverter<TicketLineId>]
internal sealed partial class ProbeIdConverters;
