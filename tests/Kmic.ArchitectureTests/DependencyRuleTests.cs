using Kmic.ArchitectureTests.Rules;
using Kmic.ArchitectureTests.Support;
using Shouldly;

namespace Kmic.ArchitectureTests;

public sealed class DependencyRuleTests
{
    [Fact]
    public void EveryProductAssembly_OnlyReferencesWhatItsLayerAllows()
    {
        var violations = ProductAssemblies.All.SelectMany(a =>
            DependencyRules.Check(a.Module, a.Layer, a.Assembly.GetReferencedAssemblies().Select(n => n.Name!)));

        violations.ShouldBeEmpty();
    }

    [Fact]
    public void ProductAssemblies_IncludeTheFrameworkAndTheSampleModule()
    {
        // 若未能扫描到程序集，上述校验将空跑通过；此处先确认扫描结果非空
        ProductAssemblies.Framework.Select(a => a.Layer).Distinct().Count().ShouldBe(5);
        ProductAssemblies.ModuleNames.ShouldContain("Sample", StringComparer.Ordinal);
        ProductAssemblies.Modules.Count(a => string.Equals(a.Module, "Sample", StringComparison.Ordinal)).ShouldBe(5);
    }

    [Theory]
    [InlineData("Sample", Layer.Domain, "Kmic.BuildingBlocks.Application")]
    [InlineData("Sample", Layer.Domain, "Kmic.Sample.Contracts")]
    [InlineData("Sample", Layer.Contracts, "Kmic.Sample.Domain")]
    [InlineData("Sample", Layer.Application, "Kmic.Sample.Infrastructure")]
    [InlineData("Sample", Layer.Application, "Kmic.Other.Domain")]
    [InlineData("Sample", Layer.Application, "Kmic.BuildingBlocks.Infrastructure")]
    [InlineData("Sample", Layer.Infrastructure, "Kmic.Other.Application")]
    [InlineData("Sample", Layer.Infrastructure, "Kmic.Sample.Presentation")]
    [InlineData("Sample", Layer.Presentation, "Kmic.Sample.Infrastructure")]
    [InlineData("Sample", Layer.Presentation, "Kmic.BuildingBlocks.Infrastructure")]
    [InlineData("BuildingBlocks", Layer.Application, "Kmic.BuildingBlocks.Infrastructure")]
    [InlineData("BuildingBlocks", Layer.Infrastructure, "Kmic.BuildingBlocks.Presentation")]
    [InlineData("BuildingBlocks", Layer.Presentation, "Kmic.BuildingBlocks.Infrastructure")]
    [InlineData("BuildingBlocks", Layer.Domain, "Kmic.BuildingBlocks.Contracts")]
    public void ForbiddenReference_IsReported(string module, Layer layer, string reference) =>
        DependencyRules.Check(module, layer, [reference]).ShouldHaveSingleItem();

    [Theory]
    [InlineData("Sample", Layer.Application, "Kmic.Other.Contracts")]
    [InlineData("Sample", Layer.Infrastructure, "Kmic.Other.Contracts")]
    [InlineData("Sample", Layer.Presentation, "Kmic.Sample.Application")]
    [InlineData("Sample", Layer.Contracts, "Kmic.BuildingBlocks.Domain")]
    [InlineData("BuildingBlocks", Layer.Contracts, "Kmic.BuildingBlocks.Domain")]
    public void AllowedReference_IsNotReported(string module, Layer layer, string reference) =>
        DependencyRules.Check(module, layer, [reference]).ShouldBeEmpty();

    [Fact]
    public void NonKmicReferences_AreIgnored() =>
        DependencyRules.Check("Sample", Layer.Domain, ["Vogen", "System.Runtime", "Microsoft.EntityFrameworkCore"]).ShouldBeEmpty();
}
