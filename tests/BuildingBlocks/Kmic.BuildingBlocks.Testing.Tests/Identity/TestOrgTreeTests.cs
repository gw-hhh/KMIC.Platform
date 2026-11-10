using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Testing.Identity;
using Shouldly;

namespace Kmic.BuildingBlocks.Testing.Tests.Identity;

public sealed class TestOrgTreeTests
{
    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t
        .Unit("总部", hq => hq
            .Unit("华东", east => east.Unit("上海").Unit("杭州"))
            .Unit("华南", south => south.Unit("广州"))));

    [Fact]
    public void SelfAndDescendants_IncludesAllLevelsBelow()
    {
        _org.SelfAndDescendants(_org["华东"]).ShouldBe([_org["华东"], _org["上海"], _org["杭州"]], ignoreOrder: true);
        _org.SelfAndDescendants(_org["总部"]).Count.ShouldBe(6);
        _org.SelfAndDescendants(_org["广州"]).ShouldBe([_org["广州"]]);
    }

    [Fact]
    public void Ancestors_NearestFirst() =>
        _org.Ancestors(_org["上海"]).ShouldBe([_org["华东"], _org["总部"]]);

    [Fact]
    public void UnknownUnit_Throws()
    {
        _org.Exists(OrgUnitId.New()).ShouldBeFalse();
        Should.Throw<KeyNotFoundException>(() => _org["北京"]);
        Should.Throw<KeyNotFoundException>(() => _org.Ancestors(OrgUnitId.New()));
    }

    [Fact]
    public void Create_DuplicateName_Throws() =>
        Should.Throw<ArgumentException>(() => TestOrgTree.Create(t => t.Unit("华东", e => e.Unit("华东"))));
}
