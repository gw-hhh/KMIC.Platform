using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Configuration.Domain.Preferences;

namespace Kmic.Configuration.Application.Preferences.Layers;

/// <summary>
/// 删除一个偏好在系统层或角色层的值，只删这一层：这一层之下的值（角色层之于系统层，系统层之于默认值）重新生效
/// </summary>
/// <param name="Key">偏好键</param>
/// <param name="RoleId">角色；不给就是系统层</param>
/// <param name="ExpectedVersion">调用方看到的版本号，别人改过或已经删除就返回版本冲突</param>
[RequirePermission(ConfigurationPermissions.PreferenceManage)]
[Audited]
[Idempotent]
public sealed record ResetPreferenceLayerCommand(string Key, RoleId? RoleId, long ExpectedVersion) : ICommand<VersionedResponse<PreferenceValueId>>;
