using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Configuration.Domain.Preferences;

namespace Kmic.Configuration.Application.Preferences.Layers;

/// <summary>删除系统层或角色层偏好设置，回退至下层生效值</summary>
/// <param name="Key">偏好键</param>
/// <param name="RoleId">角色 ID</param>
/// <param name="ExpectedVersion">预期版本号</param>
[RequirePermission(ConfigurationPermissions.PreferenceManage)]
[Audited]
[Idempotent]
public sealed record ResetPreferenceLayerCommand(string Key, RoleId? RoleId, long ExpectedVersion) : ICommand<VersionedResponse<PreferenceValueId>>;
