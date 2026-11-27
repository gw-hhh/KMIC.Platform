namespace Kmic.BuildingBlocks.Application.Features;

/// <summary>功能开关，配置值源自部署配置，修改后须重启应用生效。</summary>
public interface IFeatureFlags
{
    /// <summary>判断功能开关是否启用；未配置的开关默认视为关闭。</summary>
    ValueTask<bool> IsEnabledAsync(string name, CancellationToken ct);

    /// <summary>获取当前启用的开关名称列表，按名称排序。</summary>
    ValueTask<IReadOnlyList<string>> EnabledAsync(CancellationToken ct);
}
