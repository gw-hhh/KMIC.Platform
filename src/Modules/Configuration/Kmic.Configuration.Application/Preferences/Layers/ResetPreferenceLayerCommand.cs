using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Configuration.Domain.Preferences;

namespace Kmic.Configuration.Application.Preferences.Layers;

/// <summary>
/// 重置系统层或角色层的偏好
/// </summary>
/// <remarks>
/// 删除指定层的保存值，后续读取按允许层级回退。
/// </remarks>
/// <param name="Key">偏好键</param>
/// <param name="RoleId">角色 ID，未指定时操作系统层</param>
/// <param name="ExpectedVersion">预期版本号，记录已变更或删除时返回版本冲突</param>
[RequirePermission(ConfigurationPermissions.PreferenceManage)]
[Audited]
[Idempotent]
public sealed record ResetPreferenceLayerCommand(string Key, RoleId? RoleId, long ExpectedVersion) : ICommand<VersionedResponse<PreferenceValueId>>;
