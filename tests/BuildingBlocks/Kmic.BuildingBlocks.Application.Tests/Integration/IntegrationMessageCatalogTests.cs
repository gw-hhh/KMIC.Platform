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
        catalog.Describe(typeof(ThingChangedV2)).OrderingKeyOf(new ThingChangedV2(Guid.Empty, "甲")).ShouldBe("甲");
        first.OrderingKeyOf(new ThingChanged(Guid.Empty)).ShouldBeNull();
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
        thrown.Problems.ShouldContain(p => p.Contains("BuildingBlocks.ArchiveThing", StringComparison.Ordinal) && p.Contains("[OrderingKey]", StringComparison.Ordinal));
        thrown.Problems.Count.ShouldBe(7);
    }

    [Fact]
    public void Build_WrongSecretDeclarations_ListsEveryProblem()
    {
        var thrown = Should.Throw<DeclarationException>(() => IntegrationMessageCatalog.Build(
            [typeof(SecretEvent), typeof(SecretWithoutLifetime), typeof(SecretTooLong), typeof(LifetimeWithoutSecret)]));

        thrown.Problems.ShouldContain(p => p.Contains("BuildingBlocks.SecretEvent", StringComparison.Ordinal) && p.Contains("CarriesSecret", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("BuildingBlocks.SecretWithoutLifetime", StringComparison.Ordinal) && p.Contains("SecretLifetimeSeconds", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("BuildingBlocks.SecretTooLong", StringComparison.Ordinal) && p.Contains("600", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("BuildingBlocks.LifetimeWithoutSecret", StringComparison.Ordinal) && p.Contains("没有声明 CarriesSecret", StringComparison.Ordinal));
    }

    [Fact]
    public void Build_ValidSecretDeclaration_OnlyComplainsAboutModule()
    {
        // 测试程序集不属于任何平台模块，所以请求型消息总会因为模块被拒；这里只确认含密声明本身没有被挑出毛病
        var thrown = Should.Throw<DeclarationException>(() => IntegrationMessageCatalog.Build([typeof(SendCodeLike)]));

        thrown.Problems.ShouldNotContain(p => p.Contains("CarriesSecret", StringComparison.Ordinal) || p.Contains("SecretLifetimeSeconds", StringComparison.Ordinal));
        thrown.Problems.Count.ShouldBe(2);
    }

    [Fact]
    public void CarriesSecret_FollowsLifetime()
    {
        var plain = new IntegrationMessageType("Files.AttachLike", 1, IsRequest: true, typeof(AttachLike), "Files", null);

        plain.CarriesSecret.ShouldBeFalse();
        (plain with { SecretLifetimeSeconds = 300 }).CarriesSecret.ShouldBeTrue();
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
