using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Configuration.Domain.Settings;

namespace Kmic.Configuration.Application.Settings.Reset;

/// <summary>
/// 把一个系统设置恢复成代码里的默认值：删除它被修改过的值，并记一条新值为空的变更
/// </summary>
/// <param name="Key">设置键</param>
/// <param name="Reason">原因，必填，记入变更历史</param>
/// <param name="ExpectedVersion">调用方看到的版本号，别人改过就返回版本冲突</param>
[RequirePermission(ConfigurationPermissions.SettingView)]
[Audited]
[Idempotent]
public sealed record ResetSettingCommand(string Key, string Reason, long ExpectedVersion) : ICommand<VersionedResponse<SystemSettingValueId>>;
