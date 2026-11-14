using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Configuration.Domain.Settings;

namespace Kmic.Configuration.Application.Settings.Purge;

/// <summary>
/// 清除已移除声明的系统设置值
/// </summary>
/// <remarks>
/// 仍有代码声明的设置不能清除，如需重置请恢复默认值
/// </remarks>
/// <param name="Key">设置键</param>
/// <param name="Reason">原因，必填，记入变更历史</param>
/// <param name="ExpectedVersion">预期版本号，不匹配时返回版本冲突</param>
[RequirePermission(ConfigurationPermissions.SettingManage)]
[Audited]
[Idempotent]
public sealed record PurgeObsoleteSettingCommand(string Key, string Reason, long ExpectedVersion) : ICommand<VersionedResponse<SystemSettingValueId>>;
