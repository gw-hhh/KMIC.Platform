namespace Kmic.BuildingBlocks.Application.Settings;

/// <summary>框架声明的偏好族集合。</summary>
public static class PreferenceFamilies
{
    public static IReadOnlyList<PreferenceFamily> All { get; } = [PreferenceFamily.Shell, PreferenceFamily.Table];

    /// <summary>查找指定键所属的偏好族，不匹配时返回 null。</summary>
    public static PreferenceFamily? Find(string key)
    {
        ArgumentNullException.ThrowIfNull(key);

        return All.FirstOrDefault(f => f.Matches(key));
    }
}
