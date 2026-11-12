using System.Text.Json;

namespace Kmic.Configuration.Application.Preferences.Mine;

/// <summary>
/// 本人的一个偏好
/// </summary>
/// <param name="Key">键</param>
/// <param name="DisplayName">显示名称</param>
/// <param name="IsFamily">是否偏好族里的键：后端不理解它的内容，由前端按版本化格式读取</param>
/// <param name="Value">生效的值，JSON 对象</param>
/// <param name="Source">生效的值来自哪一层；没有任何一层设置过时是默认</param>
/// <param name="CanSet">本人能不能改它：声明里不允许用户层的偏好只能由管理员设置</param>
public sealed record MyPreferenceItem(string Key, string DisplayName, bool IsFamily, JsonElement Value, PreferenceSource Source, bool CanSet);
