namespace Kmic.BuildingBlocks.Application.Settings;

/// <summary>
/// 框架声明的偏好族
/// </summary>
public static class PreferenceFamilies
{
    /// <summary>
    /// 全部偏好族
    /// </summary>
    public static IReadOnlyList<PreferenceFamily> All { get; } = [PreferenceFamily.Shell, PreferenceFamily.Table];

    /// <summary>
    /// 键属于哪个偏好族；不属于任何一个时返回空
    /// </summary>
    public static PreferenceFamily? Find(string key)
    {
        ArgumentNullException.ThrowIfNull(key);

        return All.FirstOrDefault(f => f.Matches(key));
    }
}
