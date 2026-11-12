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
    /// 一个设置的值序列化后最大多少 KB：所有被修改过的值放在一个缓存项里，一起读
    /// </summary>
    [Range(1, 256)]
    public int ValueMaxKb { get; init; } = 16;
}
