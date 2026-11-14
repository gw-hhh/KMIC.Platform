using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Configuration.Application.Preferences.Layers;

/// <summary>
/// 查询偏好定义及系统、角色层设置，包含已保存值的偏好族键
/// </summary>
[RequirePermission(ConfigurationPermissions.PreferenceManage)]
public sealed record ListPreferenceDefinitionsQuery : IQuery<PreferenceDefinitionsResponse>;
