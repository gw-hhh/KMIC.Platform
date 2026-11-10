using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.BuildingBlocks.Application.Tests.Fixtures;

[RequirePermission(Permission)]
[Audited]
public sealed record SubmitThingCommand(string Name, IReadOnlyList<ThingItem> Items) : ICommand<Guid>
{
    public const string Permission = "Test.Thing.Submit";
}
