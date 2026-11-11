using Vogen;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

[EfCoreConverter<TicketId>]
[EfCoreConverter<TicketLineId>]
[EfCoreConverter<ReviewId>]
internal sealed partial class ProbeIdConverters;
