using System.Text.Json;

namespace Kmic.Configuration.Application.Preferences.Layers;

/// <summary>系统层或角色层已保存的偏好值</summary>
/// <param name="RoleId">角色层所属角色，系统层为空</param>
/// <param name="RoleName">角色名称，系统层或角色已不存在时为空</param>
public sealed record PreferenceLayerValue(JsonElement Value, long Version, Guid? RoleId, string? RoleName);
