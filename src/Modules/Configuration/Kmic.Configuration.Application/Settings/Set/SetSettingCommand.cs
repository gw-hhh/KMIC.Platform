using System.Text.Json;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Configuration.Domain.Settings;

namespace Kmic.Configuration.Application.Settings.Set;

/// <summary>
/// 修改系统设置
/// </summary>
/// <param name="Key">设置键</param>
/// <param name="Value">新的值，JSON 对象</param>
/// <param name="Reason">原因，必填，记入变更历史</param>
/// <param name="ExpectedVersion">预期版本号，尚无保存值时不传；不匹配时返回版本冲突</param>
[RequirePermission(ConfigurationPermissions.SettingView)]
[Audited]
[Idempotent]
public sealed record SetSettingCommand(string Key, JsonElement Value, string Reason, long? ExpectedVersion) : ICommand<VersionedResponse<SystemSettingValueId>>;
