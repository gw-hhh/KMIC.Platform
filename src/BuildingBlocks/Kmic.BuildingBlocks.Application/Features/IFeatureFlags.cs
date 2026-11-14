namespace Kmic.BuildingBlocks.Application.Features;

/// <summary>
/// 功能开关
/// </summary>
/// <remarks>
/// 现在开关的值来自部署配置，改了要重启生效。以后要在运行中切换，换这个接口的实现，调用方和开关名都不变
/// </remarks>
public interface IFeatureFlags
{
    /// <summary>
    /// 开关是否开启；没有配置过的开关按关闭处理
    /// </summary>
    /// <param name="name">开关名，如 Sample.NewImport</param>
    /// <param name="ct">取消令牌</param>
    ValueTask<bool> IsEnabledAsync(string name, CancellationToken ct);

    /// <summary>
    /// 当前对请求开放的开关名，按名称排序
    /// </summary>
    /// <param name="ct">取消令牌</param>
    ValueTask<IReadOnlyList<string>> EnabledAsync(CancellationToken ct);
}
