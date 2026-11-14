using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.BuildingBlocks.Application.Tests.Fixtures;

[AllowAuthenticated]
[FeatureGate(GatedThingQuery.Feature)]
public sealed record GatedThingQuery : IQuery<string>
{
    public const string Feature = "BuildingBlocks.GatedThing";
}
