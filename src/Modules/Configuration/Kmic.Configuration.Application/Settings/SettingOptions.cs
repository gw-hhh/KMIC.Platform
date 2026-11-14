using System.ComponentModel.DataAnnotations;

namespace Kmic.Configuration.Application.Settings;

/// <summary>
/// 系统设置的部署配置
/// </summary>
public sealed class SettingOptions
{
    /// <summary>
    /// 配置节
    /// </summary>
    public const string Section = "Configuration:Setting";

    /// <summary>
    /// 单个设置值序列化后的大小上限（KB）；所有已保存的设置值整体缓存
    /// </summary>
    [Range(1, 256)]
    public int ValueMaxKb { get; init; } = 16;
}
