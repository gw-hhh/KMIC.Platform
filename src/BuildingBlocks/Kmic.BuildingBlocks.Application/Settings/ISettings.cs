namespace Kmic.BuildingBlocks.Application.Settings;

public interface ISettings
{
    /// <summary>读取系统设置，未配置时返回默认值。</summary>
    ValueTask<T> GetAsync<T>(CancellationToken ct)
        where T : class, ISystemSetting<T>;
}
