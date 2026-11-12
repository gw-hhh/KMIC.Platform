using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Configuration.Application.Settings.Get;

/// <summary>
/// 查询全部系统设置：定义、默认值、当前值，每项带当前用户能不能修改
/// </summary>
[RequirePermission(ConfigurationPermissions.SettingView)]
public sealed record ListSettingsQuery : IQuery<SettingsResponse>;
