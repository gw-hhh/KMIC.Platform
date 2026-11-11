using Kmic.BuildingBlocks.Application.Numbering;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

internal static class ProbeNumbers
{
    [NumberRule("TK-{yyyy}{MM}-{seq:3}", Reset = NumberReset.Monthly)]
    public const string Ticket = "BuildingBlocks.Ticket";

    [NumberRule("RV-{yy}-{partition}-{seq:2}", Reset = NumberReset.Yearly, Partitioned = true)]
    public const string Review = "BuildingBlocks.Review";

    [NumberRule("DY{yy}{MM}{dd}-{seq:2}", Reset = NumberReset.Daily)]
    public const string Daily = "BuildingBlocks.Daily";
}
