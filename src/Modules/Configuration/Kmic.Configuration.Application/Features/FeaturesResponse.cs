namespace Kmic.Configuration.Application.Features;

/// <summary>当前已开启的功能开关列表</summary>
public sealed record FeaturesResponse(IReadOnlyList<string> Enabled);
