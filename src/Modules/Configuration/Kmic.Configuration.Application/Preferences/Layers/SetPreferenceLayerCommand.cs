using System.Text.Json;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Configuration.Domain.Preferences;

namespace Kmic.Configuration.Application.Preferences.Layers;

/// <summary>设置系统层或指定角色层的偏好值</summary>
/// <param name="Key">偏好键</param>
/// <param name="RoleId">角色 ID</param>
/// <param name="Value">值</param>
/// <param name="ExpectedVersion">预期版本号，首次设置时为空</param>
[RequirePermission(ConfigurationPermissions.PreferenceManage)]
[Audited]
[Idempotent]
public sealed record SetPreferenceLayerCommand(string Key, RoleId? RoleId, JsonElement Value, long? ExpectedVersion) : ICommand<VersionedResponse<PreferenceValueId>>;
