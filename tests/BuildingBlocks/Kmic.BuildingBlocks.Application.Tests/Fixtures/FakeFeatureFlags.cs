using Kmic.BuildingBlocks.Application.Features;

namespace Kmic.BuildingBlocks.Application.Tests.Fixtures;

/// <summary>
/// 测试用的功能开关：开了哪些由测试设定
/// </summary>
public sealed class FakeFeatureFlags : IFeatureFlags
{
    public ISet<string> On { get; } = new HashSet<string>(StringComparer.Ordinal);

    public ValueTask<bool> IsEnabledAsync(string name, CancellationToken ct) => ValueTask.FromResult(On.Contains(name));

    public ValueTask<IReadOnlyList<string>> EnabledAsync(CancellationToken ct) => ValueTask.FromResult<IReadOnlyList<string>>([.. On.Order(StringComparer.Ordinal)]);
}
