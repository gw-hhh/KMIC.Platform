namespace Kmic.BuildingBlocks.Application.Settings;

/// <summary>
/// 系统设置
/// </summary>
public interface ISettings
{
    /// <summary>
    /// 读取设置，没改过时返回默认值
    /// </summary>
    ValueTask<T> GetAsync<T>(CancellationToken ct)
        where T : class, ISystemSetting<T>;
}
