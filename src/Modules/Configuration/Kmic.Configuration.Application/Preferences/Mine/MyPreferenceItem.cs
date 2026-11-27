using System.Text.Json;

namespace Kmic.Configuration.Application.Preferences.Mine;

/// <summary>个人偏好项</summary>
/// <param name="Value">生效的值</param>
/// <param name="Source">生效值的来源</param>
/// <param name="CanSet">当前用户能否修改个人层的值</param>
public sealed record MyPreferenceItem(string Key, string DisplayName, bool IsFamily, JsonElement Value, PreferenceSource Source, bool CanSet);
