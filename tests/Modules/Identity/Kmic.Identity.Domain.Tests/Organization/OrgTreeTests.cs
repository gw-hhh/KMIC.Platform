using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Organization;
using Shouldly;

namespace Kmic.Identity.Domain.Tests.Organization;

public sealed class OrgTreeTests
{
    // 总部 ─ 华东 ─ 上海
    //      └ 华北
    private sealed record Sample(OrgTree Tree, OrgUnit Hq, OrgUnit East, OrgUnit North, OrgUnit Shanghai);

    private static Sample Build()
    {
        var tree = OrgTree.Create("总部").ShouldBeSuccess();
        var hq = tree.Root;
        var east = tree.AddUnit(hq.Id, "华东").ShouldBeSuccess();
        var north = tree.AddUnit(hq.Id, "华北").ShouldBeSuccess();
        var shanghai = tree.AddUnit(east.Id, "上海").ShouldBeSuccess();
        return new Sample(tree, hq, east, north, shanghai);
    }

    [Fact]
    public void Create_HasOnlyTheRoot_WithTheFixedTreeId()
    {
        var tree = OrgTree.Create(" 总部 ", "HQ").ShouldBeSuccess();

        tree.Id.ShouldBe(OrgTreeId.Default);
        tree.Units.ShouldHaveSingleItem();
        tree.Root.Name.ShouldBe("总部");
        tree.Root.Code.ShouldBe("HQ");
        tree.Root.ParentId.ShouldBeNull();
        tree.Root.Status.ShouldBe(OrgUnitStatus.Active);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_BlankRootName_IsRefused(string? name) =>
        OrgTree.Create(name).ShouldBeFailure(IdentityErrors.OrgUnitNameInvalid);

    [Fact]
    public void AddUnit_AssignsGrowingSortOrder_PerParent()
    {
        var s = Build();

        s.East.SortOrder.ShouldBeLessThan(s.North.SortOrder);
        s.Shanghai.SortOrder.ShouldBeGreaterThan(0);
        s.Shanghai.ParentId.ShouldBe(s.East.Id);
    }

    [Fact]
    public void AddUnit_DuplicateSiblingName_IsRefused_ButSameNameElsewhereIsFine()
    {
        var s = Build();

        s.Tree.AddUnit(s.Hq.Id, "华东").ShouldBeFailure(IdentityErrors.OrgUnitNameExists);
        s.Tree.AddUnit(s.Hq.Id, "华东 ".ToUpperInvariant()).ShouldBeFailure(IdentityErrors.OrgUnitNameExists);
        s.Tree.AddUnit(s.North.Id, "华东").ShouldBeSuccess();
    }

    [Fact]
    public void AddUnit_UnderMissingOrDisabledParent_IsRefused()
    {
        var s = Build();
        s.Tree.Disable(s.Shanghai.Id, hasEnabledUsers: false).ShouldBeSuccess();

        s.Tree.AddUnit(OrgUnitId.New(), "新").ShouldBeFailure(IdentityErrors.OrgUnitNotFound);
        s.Tree.AddUnit(s.Shanghai.Id, "新").ShouldBeFailure(IdentityErrors.OrgUnitDisabled);
    }

    [Fact]
    public void SelfAndDescendants_And_Ancestors_FollowTheTree()
    {
        var s = Build();

        s.Tree.SelfAndDescendants(s.East.Id).ShouldBe([s.East.Id, s.Shanghai.Id], ignoreOrder: true);
        s.Tree.SelfAndDescendants(s.Hq.Id).Count.ShouldBe(4);
        s.Tree.SelfAndDescendants(OrgUnitId.New()).ShouldBeEmpty();
        s.Tree.Ancestors(s.Shanghai.Id).ShouldBe([s.East.Id, s.Hq.Id]);
        s.Tree.Ancestors(s.Hq.Id).ShouldBeEmpty();
    }

    [Fact]
    public void Move_ToAnotherParent_Works_AndKeepsTheNameUnique()
    {
        var s = Build();

        s.Tree.Move(s.Shanghai.Id, s.North.Id).ShouldBeSuccess();

        s.Shanghai.ParentId.ShouldBe(s.North.Id);
        s.Tree.SelfAndDescendants(s.East.Id).ShouldBe([s.East.Id]);
        var other = s.Tree.AddUnit(s.Hq.Id, "上海").ShouldBeSuccess();
        s.Tree.Move(other.Id, s.North.Id).ShouldBeFailure(IdentityErrors.OrgUnitNameExists);
    }

    [Fact]
    public void Move_UnderSelfOrDescendant_IsACycle()
    {
        var s = Build();

        s.Tree.Move(s.East.Id, s.East.Id).ShouldBeFailure(IdentityErrors.OrgUnitCycle);
        s.Tree.Move(s.East.Id, s.Shanghai.Id).ShouldBeFailure(IdentityErrors.OrgUnitCycle);
    }

    [Fact]
    public void Move_Root_IsRefused()
    {
        var s = Build();

        s.Tree.Move(s.Hq.Id, s.North.Id).ShouldBeFailure(IdentityErrors.OrgRootImmutable);
        s.Tree.Move(s.Hq.Id, OrgUnitId.New()).ShouldBeFailure(IdentityErrors.OrgUnitNotFound);
    }

    [Fact]
    public void Disable_WithEnabledChildrenOrUsers_IsInUse()
    {
        var s = Build();

        s.Tree.Disable(s.East.Id, hasEnabledUsers: false).ShouldBeFailure(IdentityErrors.OrgUnitInUse);
        s.Tree.Disable(s.Shanghai.Id, hasEnabledUsers: true).ShouldBeFailure(IdentityErrors.OrgUnitInUse);
        s.Tree.Disable(s.Hq.Id, hasEnabledUsers: false).ShouldBeFailure(IdentityErrors.OrgRootImmutable);
    }

    [Fact]
    public void Disable_Bottom_UpThenEnable_Top_Down()
    {
        var s = Build();

        s.Tree.Disable(s.Shanghai.Id, false).ShouldBeSuccess();
        s.Tree.Disable(s.East.Id, false).ShouldBeSuccess();

        s.Tree.Enable(s.Shanghai.Id).ShouldBeFailure(IdentityErrors.OrgUnitDisabled);
        s.Tree.Enable(s.East.Id).ShouldBeSuccess();
        s.Tree.Enable(s.Shanghai.Id).ShouldBeSuccess();
        s.Shanghai.Status.ShouldBe(OrgUnitStatus.Active);
    }

    [Fact]
    public void Rename_And_ChangeCode_Apply()
    {
        var s = Build();

        s.Tree.Rename(s.East.Id, "东区").ShouldBeSuccess();
        s.Tree.ChangeCode(s.East.Id, "E").ShouldBeSuccess();
        s.Tree.Rename(s.East.Id, "华北").ShouldBeFailure(IdentityErrors.OrgUnitNameExists);
        s.Tree.ChangeCode(s.East.Id, "has space").IsFailure.ShouldBeTrue();

        s.East.Name.ShouldBe("东区");
        s.East.Code.ShouldBe("E");
    }

    [Fact]
    public void Reorder_RequiresExactlyTheCurrentChildren()
    {
        var s = Build();

        s.Tree.Reorder(s.Hq.Id, [s.North.Id, s.East.Id]).ShouldBeSuccess();

        s.North.SortOrder.ShouldBeLessThan(s.East.SortOrder);
        s.Tree.Reorder(s.Hq.Id, [s.North.Id]).ShouldBeFailure(IdentityErrors.OrgUnitOrderInvalid);
        s.Tree.Reorder(s.Hq.Id, [s.North.Id, s.North.Id]).ShouldBeFailure(IdentityErrors.OrgUnitOrderInvalid);
        s.Tree.Reorder(s.Hq.Id, [s.North.Id, s.Shanghai.Id]).ShouldBeFailure(IdentityErrors.OrgUnitOrderInvalid);
        s.Tree.Reorder(OrgUnitId.New(), []).ShouldBeFailure(IdentityErrors.OrgUnitNotFound);
    }
}
