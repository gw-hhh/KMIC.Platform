using System.Text.Json;

namespace Kmic.Configuration.Presentation.Settings;

/// <summary>
/// 修改系统设置的请求
/// </summary>
/// <param name="Value">新的值，JSON 对象；成员见设置里的字段清单，通不过校验时错误按成员名返回</param>
/// <param name="Reason">原因，必填，最多 500 个字符，记入变更历史</param>
/// <param name="ExpectedVersion">调用方看到的版本号；设置还在用默认值时不给。别人改过就返回 409 Common.VersionConflict</param>
public sealed record SetSettingRequest(JsonElement Value, string Reason, long? ExpectedVersion);
