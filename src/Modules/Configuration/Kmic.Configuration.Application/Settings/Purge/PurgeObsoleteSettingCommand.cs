using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Configuration.Domain.Settings;

namespace Kmic.Configuration.Application.Settings.Purge;

/// <summary>
/// 清除代码里已经不存在的设置留下的值，并记一条变更
/// </summary>
/// <remarks>
/// 设置从代码里删掉之后，值行还留着，管理页标“已不存在”；确认不再需要时用它清除。仍然存在的设置不能这样清，要用恢复默认
/// </remarks>
/// <param name="Key">设置键</param>
/// <param name="Reason">原因，必填，记入变更历史</param>
/// <param name="ExpectedVersion">调用方看到的版本号，别人改过就返回版本冲突</param>
[RequirePermission(ConfigurationPermissions.SettingManage)]
[Audited]
[Idempotent]
public sealed record PurgeObsoleteSettingCommand(string Key, string Reason, long ExpectedVersion) : ICommand<VersionedResponse<SystemSettingValueId>>;
