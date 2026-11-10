using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Domain.Results;
using Shouldly;

namespace Kmic.BuildingBlocks.Application.Tests.Settings;

public sealed class PreferenceCatalogTests
{
    [Fact]
    public void Build_ValidPreference_ReadsLayersAndDefault()
    {
        var catalog = PreferenceCatalog.Build([typeof(ListPageSize), typeof(SystemOnlyTheme)]);

        catalog.Get<ListPageSize>().Layers.ShouldBe(PreferenceLayers.All);
        catalog.Get<ListPageSize>().DefaultValue.ShouldBe(new ListPageSize(20));
        catalog.Get<ListPageSize>().Validate(new ListPageSize(500)).IsFailure.ShouldBeTrue();
        catalog.Get<SystemOnlyTheme>().Layers.ShouldBe(PreferenceLayers.System);
    }

    [Fact]
    public void Build_NoLayers_Refuses()
    {
        var thrown = Should.Throw<DeclarationException>(() => PreferenceCatalog.Build([typeof(NoLayers)]));

        thrown.Problems.ShouldContain(p => p.Contains(nameof(NoLayers), StringComparison.Ordinal) && p.Contains("Layers", StringComparison.Ordinal));
    }

    [Preference("BuildingBlocks.List.PageSize", "列表每页条数")]
    public sealed record ListPageSize(int Value) : IPreference<ListPageSize>
    {
        public static ListPageSize Default => new(20);

        public static Result Validate(ListPageSize value) =>
            value.Value is >= 10 and <= 200 ? Result.Success() : Error.BusinessRule("BuildingBlocks.PageSizeOutOfRange", "每页条数超出范围");
    }

    [Preference("BuildingBlocks.Theme", "主题", Layers = PreferenceLayers.System)]
    public sealed record SystemOnlyTheme(string Name) : IPreference<SystemOnlyTheme>
    {
        public static SystemOnlyTheme Default => new("light");

        public static Result Validate(SystemOnlyTheme value) => Result.Success();
    }

    [Preference("BuildingBlocks.NoLayers", "没有层", Layers = PreferenceLayers.None)]
    public sealed record NoLayers(int Value) : IPreference<NoLayers>
    {
        public static NoLayers Default => new(1);

        public static Result Validate(NoLayers value) => Result.Success();
    }
}
