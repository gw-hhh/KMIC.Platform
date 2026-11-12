using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.Sample.Domain.Errors;
using Kmic.Sample.Domain.Specimens;
using Kmic.Sample.Domain.Specimens.Events;
using Shouldly;

namespace Kmic.Sample.Domain.Tests.Specimens;

public sealed class SpecimenTests
{
    private static Specimen Draft() => Specimen.Create("SP-202611-0001", "标题", UserId.New(), OrgUnitId.New());

    [Fact]
    public void Create_StartsAsDraft_AndRecordsTheInitialState()
    {
        var specimen = Draft();

        specimen.Status.ShouldBe(SpecimenStatus.Draft);
        specimen.StateTransitions.ShouldHaveSingleItem();
        specimen.Items.ShouldBeEmpty();
    }

    [Fact]
    public void AddItem_OnADraft_AddsIt()
    {
        var specimen = Draft();

        var added = specimen.AddItem("样品", 3).ShouldBeSuccess();

        added.Name.ShouldBe("样品");
        added.Quantity.ShouldBe(3);
        specimen.Items.ShouldHaveSingleItem();
    }

    [Fact]
    public void Submit_WithoutItems_IsRefused()
    {
        var specimen = Draft();

        specimen.Submit().ShouldBeFailure(SampleErrors.NoItems);

        specimen.Status.ShouldBe(SpecimenStatus.Draft);
        specimen.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public void Submit_WithItems_MovesToSubmitted_AndPublishesTheEvent()
    {
        var specimen = Draft();
        specimen.AddItem("样品", 1);

        specimen.Submit("提交测试").ShouldBeSuccess();

        specimen.Status.ShouldBe(SpecimenStatus.Submitted);
        specimen.DomainEvents.ShouldHaveSingleItem().ShouldBe(new SpecimenSubmitted(specimen.Id));
        specimen.StateTransitions.Count.ShouldBe(2);
    }

    [Fact]
    public void SubmittedSpecimen_CannotBeSubmittedAgain_NorGetNewItems()
    {
        var specimen = Draft();
        specimen.AddItem("样品", 1);
        specimen.Submit();

        specimen.Submit().ShouldBeFailure(SampleErrors.InvalidState);
        specimen.AddItem("另一个", 1).ShouldBeFailure(SampleErrors.InvalidState);
    }

    [Fact]
    public void Close_OnlyAfterSubmit()
    {
        var specimen = Draft();
        specimen.AddItem("样品", 1);

        specimen.Close().ShouldBeFailure(SampleErrors.InvalidState);
        specimen.Submit();
        specimen.Close().ShouldBeSuccess();

        specimen.Status.ShouldBe(SpecimenStatus.Closed);
    }

    [Fact]
    public void CanFire_FollowsTheStateMachine()
    {
        var specimen = Draft();
        specimen.AddItem("样品", 1);

        specimen.CanFire(SpecimenAction.Submit).IsSuccess.ShouldBeTrue();
        specimen.CanFire(SpecimenAction.Close).IsSuccess.ShouldBeFalse();
        specimen.Submit();
        specimen.CanFire(SpecimenAction.Submit).IsSuccess.ShouldBeFalse();
        specimen.CanFire(SpecimenAction.Close).IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void Id_IsNeverEmpty()
    {
        Should.Throw<Exception>(() => SpecimenId.From(Guid.Empty));
        Draft().Id.Value.ShouldNotBe(Guid.Empty);
    }
}
