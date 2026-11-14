using Kmic.BuildingBlocks.Application.Features;
using Kmic.BuildingBlocks.Infrastructure.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Features;

/// <summary>
/// 功能开关：值来自 Kmic:Features，开关名里带点（模块.功能）也能读到；没有配置的按关闭处理
/// </summary>
public sealed class FeatureFlagsTests
{
    private static ServiceProvider Build(Dictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        return new ServiceCollection().AddKmicFeatures(configuration).BuildServiceProvider();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task ASwitchIsOnOrOff_AsConfigured_AndOneThatWasNeverConfiguredIsOff()
    {
        await using var provider = Build(new(StringComparer.Ordinal)
        {
            ["Kmic:Features:Sample.NewImport"] = "true",
            ["Kmic:Features:Sample.Preview"] = "false",
        });
        await using var scope = provider.CreateAsyncScope();
        var features = scope.ServiceProvider.GetRequiredService<IFeatureFlags>();

        (await features.IsEnabledAsync("Sample.NewImport", Ct)).ShouldBeTrue();
        (await features.IsEnabledAsync("Sample.Preview", Ct)).ShouldBeFalse();
        (await features.IsEnabledAsync("Sample.NeverConfigured", Ct)).ShouldBeFalse();
    }

    [Fact]
    public async Task TheEnabledList_HasOnlyTheOnesThatAreOn_SortedByName()
    {
        await using var provider = Build(new(StringComparer.Ordinal)
        {
            ["Kmic:Features:Sample.Zeta"] = "true",
            ["Kmic:Features:Sample.Off"] = "false",
            ["Kmic:Features:Orders.Alpha"] = "true",
        });
        await using var scope = provider.CreateAsyncScope();

        var enabled = await scope.ServiceProvider.GetRequiredService<IFeatureFlags>().EnabledAsync(Ct);

        enabled.ShouldBe(["Orders.Alpha", "Sample.Zeta"]);
    }

    [Fact]
    public async Task WithNothingConfigured_NothingIsOn()
    {
        await using var provider = Build(new(StringComparer.Ordinal));
        await using var scope = provider.CreateAsyncScope();

        (await scope.ServiceProvider.GetRequiredService<IFeatureFlags>().EnabledAsync(Ct)).ShouldBeEmpty();
    }

    [Fact]
    public async Task ABlankName_IsAnError_NotQuietlyOff()
    {
        await using var provider = Build(new(StringComparer.Ordinal));
        await using var scope = provider.CreateAsyncScope();

        await Should.ThrowAsync<ArgumentException>(async () => await scope.ServiceProvider.GetRequiredService<IFeatureFlags>().IsEnabledAsync(" ", Ct));
    }
}
