namespace Kmic.Configuration.Application.Features;

/// <summary>
/// 当前对请求开放的功能开关
/// </summary>
/// <param name="Enabled">开启的开关名，按名称排序；没有配置的开关按关闭处理，不在列表里</param>
public sealed record FeaturesResponse(IReadOnlyList<string> Enabled);
