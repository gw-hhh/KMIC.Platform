using System.Text.Json;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Serialization;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Shouldly;

namespace Kmic.BuildingBlocks.Application.Tests.Messaging;

public sealed class CreatedResponseTests
{
    [Fact]
    public void WithoutAnOwner_TheVersionBelongsToTheNewDataItself()
    {
        var id = UserId.New();

        new CreatedResponse<UserId>(id).AggregateId.ShouldBe(id.Value);
    }

    [Fact]
    public void WithAnOwner_TheVersionBelongsToTheOwner()
    {
        var unit = OrgUnitId.New();
        var tree = Guid.CreateVersion7();

        new CreatedResponse<OrgUnitId>(unit, OwnerId: tree).AggregateId.ShouldBe(tree);
    }

    [Fact]
    public void WithConcurrencyVersion_KeepsTheOwner()
    {
        var tree = Guid.CreateVersion7();
        var stamped = (CreatedResponse<OrgUnitId>)new CreatedResponse<OrgUnitId>(OrgUnitId.New(), OwnerId: tree).WithConcurrencyVersion(7);

        stamped.ConcurrencyVersion.ShouldBe(7);
        stamped.AggregateId.ShouldBe(tree);
    }

    [Fact]
    public void TheOwner_IsNotPartOfTheJson()
    {
        var unit = OrgUnitId.New();

        var json = JsonSerializer.Serialize(new CreatedResponse<OrgUnitId>(unit, 3, Guid.CreateVersion7()), KmicJsonOptions.Default);

        json.ShouldBe($$"""{"id":"{{unit.Value}}","version":3}""");
    }
}
