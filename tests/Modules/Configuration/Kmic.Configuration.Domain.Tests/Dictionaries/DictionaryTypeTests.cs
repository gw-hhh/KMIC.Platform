using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.Configuration.Domain.Dictionaries;
using Kmic.Configuration.Domain.Errors;
using Shouldly;

namespace Kmic.Configuration.Domain.Tests.Dictionaries;

public sealed class DictionaryTypeTests
{
    private const int Max = 50;

    private static DictionaryType Flat() => DictionaryType.Register("Sample.Industry", "Sample", "行业", isHierarchical: false);

    private static DictionaryType Tree() => DictionaryType.Register("Sample.Region", "Sample", "地区", isHierarchical: true);

    [Fact]
    public void AddItem_AppendsAtTheEndOfTheLevel_InSteps()
    {
        var type = Flat();

        var first = type.AddItem("auto", "汽车", null, null, Max).ShouldBeSuccess();
        var second = type.AddItem("chem", "化工", null, null, Max).ShouldBeSuccess();

        first.SortOrder.ShouldBe(10);
        second.SortOrder.ShouldBe(20);
        first.Status.ShouldBe(DictionaryItemStatus.Active);
        type.Items.Count.ShouldBe(2);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("has space")]
    [InlineData("中文")]
    [InlineData("a/b")]
    public void AddItem_RefusesCodesWithTheWrongShape(string code) =>
        Flat().AddItem(code, "名称", null, null, Max).ShouldBeFailure(ConfigurationErrors.DictionaryItemCodeInvalid);

    [Fact]
    public void AddItem_RefusesACodeLongerThanFiftyCharacters() =>
        Flat().AddItem(new string('a', 51), "名称", null, null, Max).ShouldBeFailure(ConfigurationErrors.DictionaryItemCodeInvalid);

    [Fact]
    public void AddItem_CodesAreUniqueIgnoringCase()
    {
        var type = Flat();
        type.AddItem("Auto", "汽车", null, null, Max).ShouldBeSuccess();

        type.AddItem("AUTO", "又一个汽车", null, null, Max).ShouldBeFailure(ConfigurationErrors.DictionaryItemCodeExists);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddItem_NeedsAName(string? name) =>
        Flat().AddItem("auto", name, null, null, Max).ShouldBeFailure(ConfigurationErrors.DictionaryItemNameInvalid);

    [Fact]
    public void AddItem_StopsAtTheLimit()
    {
        var type = Flat();
        type.AddItem("a", "甲", null, null, 2).ShouldBeSuccess();
        type.AddItem("b", "乙", null, null, 2).ShouldBeSuccess();

        type.AddItem("c", "丙", null, null, 2).ShouldBeFailure(ConfigurationErrors.DictionaryTooManyItems);
    }

    [Fact]
    public void AFlatDictionary_RefusesAParent()
    {
        var type = Flat();
        type.AddItem("auto", "汽车", null, null, Max).ShouldBeSuccess();

        type.AddItem("suv", "SUV", "auto", null, Max).ShouldBeFailure(ConfigurationErrors.DictionaryParentInvalid);
    }

    [Fact]
    public void AHierarchicalDictionary_NeedsAnExistingEnabledParent()
    {
        var type = Tree();
        type.AddItem("east", "华东", null, null, Max).ShouldBeSuccess();
        type.AddItem("sh", "上海", "east", null, Max).ShouldBeSuccess();
        type.DisableItem("sh").ShouldBeSuccess();
        type.DisableItem("east").ShouldBeSuccess();

        type.AddItem("pd", "浦东", "nowhere", null, Max).ShouldBeFailure(ConfigurationErrors.DictionaryParentInvalid);
        type.AddItem("pd", "浦东", "east", null, Max).ShouldBeFailure(ConfigurationErrors.DictionaryParentInvalid);
    }

    [Fact]
    public void Rename_ChangesOnlyTheName_NeverTheCode()
    {
        var type = Flat();
        type.AddItem("auto", "汽车", null, null, Max).ShouldBeSuccess();

        type.RenameItem("AUTO", " 汽车行业 ").ShouldBeSuccess();

        var item = type.FindItem("auto")!;
        item.Name.ShouldBe("汽车行业");
        item.Code.ShouldBe("auto");
        type.RenameItem("auto", " ").ShouldBeFailure(ConfigurationErrors.DictionaryItemNameInvalid);
        type.RenameItem("ghost", "没有").ShouldBeFailure(ConfigurationErrors.DictionaryItemNotFound);
    }

    [Fact]
    public void Move_PutsTheItemLastUnderTheNewParent_AndCanMoveToTheTop()
    {
        var type = Tree();
        type.AddItem("east", "华东", null, null, Max).ShouldBeSuccess();
        type.AddItem("south", "华南", null, null, Max).ShouldBeSuccess();
        type.AddItem("sh", "上海", "east", null, Max).ShouldBeSuccess();
        type.AddItem("gz", "广州", "south", null, Max).ShouldBeSuccess();

        type.MoveItem("sh", "south").ShouldBeSuccess();

        type.FindItem("sh")!.ParentCode.ShouldBe("south");
        type.FindItem("sh")!.SortOrder.ShouldBe(20);
        type.MoveItem("sh", null).ShouldBeSuccess();
        type.FindItem("sh")!.ParentCode.ShouldBeNull();
    }

    [Fact]
    public void Move_CannotMakeACycle()
    {
        var type = Tree();
        type.AddItem("a", "甲", null, null, Max).ShouldBeSuccess();
        type.AddItem("b", "乙", "a", null, Max).ShouldBeSuccess();
        type.AddItem("c", "丙", "b", null, Max).ShouldBeSuccess();

        type.MoveItem("a", "c").ShouldBeFailure(ConfigurationErrors.DictionaryParentInvalid);
        type.MoveItem("a", "a").ShouldBeFailure(ConfigurationErrors.DictionaryParentInvalid);
        type.MoveItem("b", "a").ShouldBeSuccess();
    }

    [Fact]
    public void Reorder_NeedsTheWholeLevel_AndRenumbersInSteps()
    {
        var type = Flat();
        type.AddItem("a", "甲", null, null, Max).ShouldBeSuccess();
        type.AddItem("b", "乙", null, null, Max).ShouldBeSuccess();
        type.AddItem("c", "丙", null, null, Max).ShouldBeSuccess();

        type.ReorderItems(null, ["a", "b"]).ShouldBeFailure(ConfigurationErrors.DictionaryOrderInvalid);
        type.ReorderItems(null, ["a", "b", "b"]).ShouldBeFailure(ConfigurationErrors.DictionaryOrderInvalid);
        type.ReorderItems(null, ["a", "b", "x"]).ShouldBeFailure(ConfigurationErrors.DictionaryOrderInvalid);
        type.ReorderItems(null, ["c", "a", "b"]).ShouldBeSuccess();

        type.FindItem("c")!.SortOrder.ShouldBe(10);
        type.FindItem("a")!.SortOrder.ShouldBe(20);
        type.FindItem("b")!.SortOrder.ShouldBe(30);
    }

    [Fact]
    public void Disable_IsRefusedWhileThereAreEnabledChildren_AndEnableNeedsAnEnabledParent()
    {
        var type = Tree();
        type.AddItem("east", "华东", null, null, Max).ShouldBeSuccess();
        type.AddItem("sh", "上海", "east", null, Max).ShouldBeSuccess();

        type.DisableItem("east").ShouldBeFailure(ConfigurationErrors.DictionaryItemHasEnabledChildren);
        type.DisableItem("sh").ShouldBeSuccess();
        type.DisableItem("east").ShouldBeSuccess();
        type.EnableItem("sh").ShouldBeFailure(ConfigurationErrors.DictionaryParentInvalid);
        type.EnableItem("east").ShouldBeSuccess();
        type.EnableItem("sh").ShouldBeSuccess();
    }

    [Fact]
    public void DisablingTwice_OrEnablingAnEnabledItem_IsAnInvalidState()
    {
        var type = Flat();
        type.AddItem("auto", "汽车", null, null, Max).ShouldBeSuccess();

        type.EnableItem("auto").ShouldBeFailure(ConfigurationErrors.InvalidState);
        type.DisableItem("auto").ShouldBeSuccess();
        type.DisableItem("auto").ShouldBeFailure(ConfigurationErrors.InvalidState);
    }

    [Fact]
    public void IsSelectable_IsTrueOnlyForEnabledItems()
    {
        var type = Flat();
        type.AddItem("auto", "汽车", null, null, Max).ShouldBeSuccess();
        type.AddItem("old", "旧行业", null, null, Max).ShouldBeSuccess();
        type.DisableItem("old").ShouldBeSuccess();

        type.IsSelectable("AUTO").ShouldBeTrue();
        type.IsSelectable("old").ShouldBeFalse();
        type.IsSelectable("ghost").ShouldBeFalse();
        type.IsSelectable(null).ShouldBeFalse();
    }

    [Fact]
    public void ARetiredType_CannotBeEdited_UntilItIsDeclaredAgain()
    {
        var type = Flat();
        type.AddItem("auto", "汽车", null, null, Max).ShouldBeSuccess();

        type.Retire().ShouldBeTrue();
        type.Retire().ShouldBeFalse();

        type.AddItem("chem", "化工", null, null, Max).ShouldBeFailure(ConfigurationErrors.DictionaryTypeRetired);
        type.RenameItem("auto", "新名").ShouldBeFailure(ConfigurationErrors.DictionaryTypeRetired);
        type.DisableItem("auto").ShouldBeFailure(ConfigurationErrors.DictionaryTypeRetired);
        type.Synchronize("行业", isHierarchical: false).ShouldBeTrue();
        type.Status.ShouldBe(DictionaryTypeStatus.Active);
        type.AddItem("chem", "化工", null, null, Max).ShouldBeSuccess();
    }

    [Fact]
    public void Synchronize_ReportsWhetherAnythingChanged()
    {
        var type = Flat();

        type.Synchronize("行业", isHierarchical: false).ShouldBeFalse();
        type.Synchronize("客户行业", isHierarchical: false).ShouldBeTrue();
        type.DisplayName.ShouldBe("客户行业");
        type.Synchronize("客户行业", isHierarchical: true).ShouldBeTrue();
        type.IsHierarchical.ShouldBeTrue();
    }
}
