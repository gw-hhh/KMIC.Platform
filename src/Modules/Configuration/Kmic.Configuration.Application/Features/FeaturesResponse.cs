namespace Kmic.Configuration.Application.Features;

/// <summary>
/// 当前已开启的功能开关
/// </summary>
/// <param name="Enabled">开启的开关名</param>
public sealed record FeaturesResponse(IReadOnlyList<string> Enabled);
