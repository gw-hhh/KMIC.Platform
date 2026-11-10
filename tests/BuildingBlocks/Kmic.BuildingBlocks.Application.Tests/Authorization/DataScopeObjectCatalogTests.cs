using Kmic.BuildingBlocks.Application.Authorization;
using Shouldly;

namespace Kmic.BuildingBlocks.Application.Tests.Authorization;

public sealed class DataScopeObjectCatalogTests
{
    [Fact]
    public void Build_ValidGroup_ReadsDefinitions()
    {
        var catalog = DataScopeObjectCatalog.Build([typeof(GoodScopes)]);

        catalog.All.ShouldBe([new DataScopeObjectDefinition(GoodScopes.Thing, "Thing", "东西")]);
        catalog.Contains(GoodScopes.Thing).ShouldBeTrue();
        catalog.Contains("Thing.Other").ShouldBeFalse();
    }

    [Fact]
    public void Build_WrongCodes_ReportsEachProblem()
    {
        var thrown = Should.Throw<DeclarationException>(() => DataScopeObjectCatalog.Build([typeof(BadScopes), typeof(GoodScopes), typeof(DuplicateScopes)]));

        thrown.Problems.ShouldContain(p => p.Contains("Thing.Bad.Three", StringComparison.Ordinal) && p.Contains("格式", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("Other.Thing", StringComparison.Ordinal) && p.Contains("本模块名", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("NotConstant", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("Thing.Thing 重复", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("“东西”重复", StringComparison.Ordinal));
    }

    [DataScopeObjectGroup("Thing")]
    public static class GoodScopes
    {
        [DataScopeObject("东西")]
        public const string Thing = "Thing.Thing";
    }

    [DataScopeObjectGroup("Thing")]
    public static class BadScopes
    {
        [DataScopeObject("三段")]
        public const string ThreeSegments = "Thing.Bad.Three";

        [DataScopeObject("别的模块")]
        public const string OtherModule = "Other.Thing";

        public static readonly string NotConstant = "Thing.Field";
    }

    [DataScopeObjectGroup("Thing")]
    public static class DuplicateScopes
    {
        [DataScopeObject("东西")]
        public const string Thing = "Thing.Thing";
    }
}
