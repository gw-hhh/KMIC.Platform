using System.ComponentModel.DataAnnotations;

namespace Kmic.Configuration.Application.Preferences;

/// <summary>
/// 偏好的部署配置
/// </summary>
public sealed class PreferenceOptions
{
    /// <summary>
    /// 配置节
    /// </summary>
    public const string Section = "Configuration:Preference";

    /// <summary>
    /// 偏好族的一个值最大多少 KB：后端不理解族里的内容，只限制大小
    /// </summary>
    [Range(1, 256)]
    public int FamilyValueMaxKb { get; init; } = 16;
}
