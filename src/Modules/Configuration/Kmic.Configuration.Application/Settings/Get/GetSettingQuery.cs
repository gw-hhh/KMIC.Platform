using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Configuration.Application.Settings.Get;

/// <summary>
/// 查询一个系统设置
/// </summary>
/// <param name="Key">设置键</param>
[RequirePermission(ConfigurationPermissions.SettingView)]
public sealed record GetSettingQuery(string Key) : IQuery<SettingItem>;
