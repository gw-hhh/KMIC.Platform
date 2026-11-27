using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Configuration.Application.Settings.Get;

/// <summary>查询单个系统设置详情</summary>
[RequirePermission(ConfigurationPermissions.SettingView)]
public sealed record GetSettingQuery(string Key) : IQuery<SettingItem>;
