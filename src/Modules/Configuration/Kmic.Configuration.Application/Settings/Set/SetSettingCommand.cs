using System.Text.Json;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Configuration.Domain.Settings;

namespace Kmic.Configuration.Application.Settings.Set;

/// <summary>
/// 修改一个系统设置
/// </summary>
/// <remarks>
/// 命令声明“查看设置”这个基础权限点，再由设置的资源策略检查该设置自己声明的修改权限点。
/// 值是 JSON 对象，按设置的值类型读出后执行设置自己的校验，通不过返回逐字段的错误
/// </remarks>
/// <param name="Key">设置键</param>
/// <param name="Value">新的值，JSON 对象</param>
/// <param name="Reason">原因，必填，记入变更历史</param>
/// <param name="ExpectedVersion">调用方看到的版本号；设置还在用默认值时不给。别人改过就返回版本冲突</param>
[RequirePermission(ConfigurationPermissions.SettingView)]
[Audited]
[Idempotent]
public sealed record SetSettingCommand(string Key, JsonElement Value, string Reason, long? ExpectedVersion) : ICommand<VersionedResponse<SystemSettingValueId>>;
