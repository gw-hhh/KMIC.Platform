using Kmic.BuildingBlocks.Application.Dictionaries;
using Shouldly;

namespace Kmic.BuildingBlocks.Application.Tests.Dictionaries;

public sealed class DictionaryTypeCatalogTests
{
    [Fact]
    public void Build_ValidGroup_ReadsDefinitions()
    {
        var catalog = DictionaryTypeCatalog.Build([typeof(GoodDictionaries)]);

        catalog.All.ShouldBe(
            [
                new DictionaryTypeDefinition(GoodDictionaries.Industry, "Thing", "行业", IsHierarchical: false),
                new DictionaryTypeDefinition(GoodDictionaries.Region, "Thing", "地区", IsHierarchical: true),
            ],
            ignoreOrder: true);
        catalog.Contains(GoodDictionaries.Industry).ShouldBeTrue();
        catalog.Contains("Thing.Other").ShouldBeFalse();
    }

    [Fact]
    public void Build_WrongDeclarations_ReportsEachProblem()
    {
        var thrown = Should.Throw<DeclarationException>(() =>
            DictionaryTypeCatalog.Build([typeof(BadDictionaries), typeof(GoodDictionaries), typeof(DuplicateDictionaries)]));

        thrown.Problems.ShouldContain(p => p.Contains("thing.lower", StringComparison.Ordinal) && p.Contains("格式", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("Other.Unit", StringComparison.Ordinal) && p.Contains("本模块名", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("NotConstant", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("Thing.Industry 重复", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("“行业”重复", StringComparison.Ordinal));
    }

    [DictionaryTypeGroup("Thing")]
    public static class GoodDictionaries
    {
        [DictionaryType("行业")]
        public const string Industry = "Thing.Industry";

        [DictionaryType("地区", IsHierarchical = true)]
        public const string Region = "Thing.Region";
    }

    [DictionaryTypeGroup("Thing")]
    public static class BadDictionaries
    {
        [DictionaryType("小写")]
        public const string Lower = "thing.lower";

        [DictionaryType("别的模块")]
        public const string Foreign = "Other.Unit";

        [DictionaryType("不是常量")]
        public static readonly string NotConstant = "Thing.NotConstant";
    }

    [DictionaryTypeGroup("Thing")]
    public static class DuplicateDictionaries
    {
        [DictionaryType("行业")]
        public const string Industry = "Thing.Industry";
    }
}
