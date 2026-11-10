namespace Kmic.BuildingBlocks.Application.Settings;

/// <summary>
/// 读取系统设置的当前值；由 Configuration 模块实现
/// </summary>
public interface ISettings
{
    /// <summary>
    /// 当前值，没有修改过时为默认值；值类型即键
    /// </summary>
    ValueTask<T> GetAsync<T>(CancellationToken cancellationToken)
        where T : class, ISystemSetting<T>;
}
