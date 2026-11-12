using System.Text.Json;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Configuration.Domain.Preferences;

namespace Kmic.Configuration.Application.Preferences.Layers;

/// <summary>
/// 设置一个偏好在系统层或角色层的值
/// </summary>
/// <remarks>
/// 不给角色就是系统层，对所有人生效；给了角色就是这个角色的默认值。用户自己没设置过的人才会看到它们
/// </remarks>
/// <param name="Key">偏好键：目录里声明过的，或属于偏好族</param>
/// <param name="RoleId">角色；不给就是系统层</param>
/// <param name="Value">值，JSON 对象</param>
/// <param name="ExpectedVersion">调用方看到的版本号；这一层还没有值时不给。别人改过就返回版本冲突</param>
[RequirePermission(ConfigurationPermissions.PreferenceManage)]
[Audited]
[Idempotent]
public sealed record SetPreferenceLayerCommand(string Key, RoleId? RoleId, JsonElement Value, long? ExpectedVersion) : ICommand<VersionedResponse<PreferenceValueId>>;
