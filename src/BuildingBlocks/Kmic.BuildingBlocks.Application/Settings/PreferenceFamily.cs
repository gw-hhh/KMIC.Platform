using System.Text.RegularExpressions;

namespace Kmic.BuildingBlocks.Application.Settings;

/// <summary>偏好族：键名带动态后缀的透传配置，后端不解析内容（用于纯前端界面偏好，如表格列配置）。</summary>
public sealed partial class PreferenceFamily
{
    private readonly Regex _keyPattern;

    private PreferenceFamily(string name, string displayName, Regex keyPattern)
    {
        Name = name;
        DisplayName = displayName;
        _keyPattern = keyPattern;
    }

    public const int MaxKeyLength = 128;

    /// <summary>族名，同时作为键的前缀。</summary>
    public string Name { get; }

    public string DisplayName { get; }

    /// <summary>判断键名格式与长度是否符合当前偏好族。</summary>
    public bool Matches(string key)
    {
        ArgumentNullException.ThrowIfNull(key);

        return key.Length <= MaxKeyLength && _keyPattern.IsMatch(key);
    }

    // 界面外壳设置（主题、侧栏、标签页等），固定单键
    internal static PreferenceFamily Shell { get; } = new("Ui.Shell", "界面外壳设置", ShellKey());

    // 表格设置：键格式为 Ui.Table.{模块}.{页面}.{表格}，各段仅含字母与数字
    internal static PreferenceFamily Table { get; } = new("Ui.Table", "表格设置", TableKey());

    [GeneratedRegex(@"^Ui\.Shell$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex ShellKey();

    [GeneratedRegex(@"^Ui\.Table(\.[A-Za-z0-9]+){3}$", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 100)]
    private static partial Regex TableKey();
}
