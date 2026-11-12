using System.Text.RegularExpressions;

namespace Kmic.BuildingBlocks.Application.Settings;

/// <summary>
/// 偏好族：一组键带后缀的偏好，后端只管键的格式和值的大小，不理解内容
/// </summary>
/// <remarks>
/// 纯前端的展示偏好（外壳设置、表格列设置）用它：每加一个表格就多一个键，不可能逐个在后端声明。
/// 内容由前端按带版本号的格式容错读取
/// </remarks>
public sealed partial class PreferenceFamily
{
    private readonly Regex _keyPattern;

    private PreferenceFamily(string name, string displayName, Regex keyPattern)
    {
        Name = name;
        DisplayName = displayName;
        _keyPattern = keyPattern;
    }

    /// <summary>
    /// 键的最大长度
    /// </summary>
    public const int MaxKeyLength = 128;

    /// <summary>
    /// 族名，也是键的开头
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 显示名称
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// 键是否属于这个族：格式对、长度没超
    /// </summary>
    public bool Matches(string key)
    {
        ArgumentNullException.ThrowIfNull(key);

        return key.Length <= MaxKeyLength && _keyPattern.IsMatch(key);
    }

    /// <summary>
    /// 外壳设置：主题、侧栏、标签页数量等，一个键
    /// </summary>
    internal static PreferenceFamily Shell { get; } = new("Ui.Shell", "界面外壳设置", ShellKey());

    /// <summary>
    /// 表格设置：键是 Ui.Table.{模块}.{页面}.{表格}，每段只有字母和数字
    /// </summary>
    internal static PreferenceFamily Table { get; } = new("Ui.Table", "表格设置", TableKey());

    [GeneratedRegex(@"^Ui\.Shell$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex ShellKey();

    [GeneratedRegex(@"^Ui\.Table(\.[A-Za-z0-9]+){3}$", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 100)]
    private static partial Regex TableKey();
}
