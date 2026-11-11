using Kmic.BuildingBlocks.Application.Integration;
using Shouldly;
using static Kmic.BuildingBlocks.Application.Tests.Integration.IntegrationMessageSamples;

namespace Kmic.BuildingBlocks.Application.Tests.Integration;

public sealed class IntegrationMessageCatalogTests
{
    [Fact]
    public void Build_SameNameNewVersion_BothDescribed()
    {
        var catalog = IntegrationMessageCatalog.Build([typeof(ThingChanged), typeof(ThingChangedV2)]);

        var first = catalog.Describe(typeof(ThingChanged));
        first.Name.ShouldBe("BuildingBlocks.ThingChanged");
        first.Version.ShouldBe(1);
        first.IsRequest.ShouldBeFalse();
        first.Module.ShouldBe("BuildingBlocks");
        catalog.Find("BuildingBlocks.ThingChanged", 2).ShouldNotBeNull().ClrType.ShouldBe(typeof(ThingChangedV2));
        catalog.Find("BuildingBlocks.ThingChanged", 3).ShouldBeNull();
    }

    [Fact]
    public void Build_WrongDeclarations_ListsEveryProblem()
    {
        var thrown = Should.Throw<DeclarationException>(() => IntegrationMessageCatalog.Build(
            [typeof(ThingChanged), typeof(Unnamed), typeof(WrongPrefix), typeof(SameNameAndVersion), typeof(ZeroVersion), typeof(ArchiveThing), typeof(EventWithSubjects)]));

        thrown.Problems.ShouldContain(p => p.Contains(nameof(Unnamed), StringComparison.Ordinal) && p.Contains("[IntegrationMessage]", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("Sales.WrongPrefix", StringComparison.Ordinal) && p.Contains("BuildingBlocks.", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("重复", StringComparison.Ordinal) && p.Contains(nameof(SameNameAndVersion), StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("BuildingBlocks.ZeroVersion", StringComparison.Ordinal) && p.Contains("版本号", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("BuildingBlocks.ArchiveThing", StringComparison.Ordinal) && p.Contains("平台模块", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("BuildingBlocks.EventWithSubjects", StringComparison.Ordinal) && p.Contains("[MessageSubjects]", StringComparison.Ordinal));
        thrown.Problems.Count.ShouldBe(6);
    }

    [Fact]
    public void Describe_NotDeclared_Throws() =>
        Should.Throw<InvalidOperationException>(() => IntegrationMessageCatalog.Empty.Describe(typeof(ThingChanged)));

    [Fact]
    public void SubjectIdsOf_SingleIdOrCollection()
    {
        var message = new AttachLike(Guid.CreateVersion7(), [Guid.CreateVersion7(), Guid.CreateVersion7()]);
        var byCollection = new IntegrationMessageType("Files.AttachLike", 1, IsRequest: true, typeof(AttachLike), "Files", typeof(AttachLike).GetProperty(nameof(AttachLike.FileIds)));
        var bySingle = byCollection with { Subjects = typeof(AttachLike).GetProperty(nameof(AttachLike.Owner)) };

        byCollection.SubjectIdsOf(message).ShouldBe(message.FileIds);
        bySingle.SubjectIdsOf(message).ShouldBe([message.Owner]);
        (byCollection with { Subjects = null }).SubjectIdsOf(message).ShouldBeNull();
    }
}
