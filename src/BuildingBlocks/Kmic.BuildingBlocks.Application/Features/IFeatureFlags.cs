namespace Kmic.BuildingBlocks.Application.Features;

/// <summary>
/// 功能开关
/// </summary>
/// <remarks>
/// 值来自部署配置，改了要重启
/// </remarks>
public interface IFeatureFlags
{
    /// <summary>
    /// 开关是否开启；没有配置过的开关按关闭处理
    /// </summary>
    ValueTask<bool> IsEnabledAsync(string name, CancellationToken ct);

    /// <summary>
    /// 当前对请求开放的开关名，按名称排序
    /// </summary>
    ValueTask<IReadOnlyList<string>> EnabledAsync(CancellationToken ct);
}
