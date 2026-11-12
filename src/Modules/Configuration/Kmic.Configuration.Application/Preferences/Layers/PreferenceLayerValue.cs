using System.Text.Json;

namespace Kmic.Configuration.Application.Preferences.Layers;

/// <summary>
/// 偏好在系统层或某个角色层上的值
/// </summary>
/// <param name="Value">值，JSON 对象</param>
/// <param name="Version">版本号，修改或删除时带回来</param>
/// <param name="RoleId">角色层所属的角色；系统层为空</param>
/// <param name="RoleName">角色名称；角色已经不存在时为空</param>
public sealed record PreferenceLayerValue(JsonElement Value, long Version, Guid? RoleId, string? RoleName);
