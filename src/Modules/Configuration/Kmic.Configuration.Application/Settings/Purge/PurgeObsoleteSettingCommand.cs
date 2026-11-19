using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Configuration.Domain.Settings;

namespace Kmic.Configuration.Application.Settings.Purge;

/// <summary>
/// 清除已移除声明的系统设置值
/// </summary>
/// <param name="Key">设置键</param>
/// <param name="Reason">原因</param>
/// <param name="ExpectedVersion">预期版本号</param>
[RequirePermission(ConfigurationPermissions.SettingManage)]
[Audited]
[Idempotent]
public sealed record PurgeObsoleteSettingCommand(string Key, string Reason, long ExpectedVersion) : ICommand<VersionedResponse<SystemSettingValueId>>;
