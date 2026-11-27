using System.ComponentModel.DataAnnotations;

namespace Kmic.Configuration.Application.Preferences;

/// <summary>偏好部署配置</summary>
public sealed class PreferenceOptions
{
    public const string Section = "Configuration:Preference";

    /// <summary>单个偏好族值的大小上限（KB），后端不按业务模型解释其内容</summary>
    [Range(1, 256)]
    public int FamilyValueMaxKb { get; init; } = 16;
}
