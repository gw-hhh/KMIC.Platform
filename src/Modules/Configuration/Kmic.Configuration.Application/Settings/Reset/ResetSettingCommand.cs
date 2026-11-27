using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Configuration.Domain.Settings;

namespace Kmic.Configuration.Application.Settings.Reset;

/// <summary>将系统设置恢复为代码默认值，删除已保存的值并记录变更</summary>
/// <param name="Key">设置键</param>
/// <param name="Reason">原因</param>
/// <param name="ExpectedVersion">预期版本号</param>
[RequirePermission(ConfigurationPermissions.SettingView)]
[Audited]
[Idempotent]
public sealed record ResetSettingCommand(string Key, string Reason, long ExpectedVersion) : ICommand<VersionedResponse<SystemSettingValueId>>;
