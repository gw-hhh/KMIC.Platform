using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Configuration.Application.Preferences.Layers;

/// <summary>
/// 管理页用的偏好清单：目录里声明的偏好、偏好族里已经有系统层或角色层值的键，各自允许的层和现有的值
/// </summary>
[RequirePermission(ConfigurationPermissions.PreferenceManage)]
public sealed record ListPreferenceDefinitionsQuery : IQuery<PreferenceDefinitionsResponse>;
