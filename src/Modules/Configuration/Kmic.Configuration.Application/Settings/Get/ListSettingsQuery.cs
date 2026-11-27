using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Configuration.Application.Settings.Get;

/// <summary>查询系统设置及已移除代码声明的残留值</summary>
[RequirePermission(ConfigurationPermissions.SettingView)]
public sealed record ListSettingsQuery : IQuery<SettingsResponse>;
