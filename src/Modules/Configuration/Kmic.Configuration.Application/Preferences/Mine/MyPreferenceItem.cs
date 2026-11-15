using System.Text.Json;

namespace Kmic.Configuration.Application.Preferences.Mine;

/// <summary>
/// 本人的偏好项
/// </summary>
/// <param name="Key">键</param>
/// <param name="DisplayName">显示名称</param>
/// <param name="IsFamily">是否偏好族键；后端仅校验 JSON 对象格式和大小，前端按版本化格式读取</param>
/// <param name="Value">生效的值，JSON 对象</param>
/// <param name="Source">生效值的来源；所有允许层均无可用值时为默认</param>
/// <param name="CanSet">当前用户是否可设置个人层的值</param>
public sealed record MyPreferenceItem(string Key, string DisplayName, bool IsFamily, JsonElement Value, PreferenceSource Source, bool CanSet);
