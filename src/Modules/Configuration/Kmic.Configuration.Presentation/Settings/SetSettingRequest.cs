using System.Text.Json;

namespace Kmic.Configuration.Presentation.Settings;

/// <summary>
/// 修改系统设置的请求
/// </summary>
/// <param name="Value">新的设置值，须为 JSON 对象；字段及校验要求见设置定义</param>
/// <param name="Reason">原因，必填，最多 500 个字符，记入变更历史</param>
/// <param name="ExpectedVersion">预期版本号，尚无保存值时不传；不匹配时返回 409 Common.VersionConflict</param>
public sealed record SetSettingRequest(JsonElement Value, string Reason, long? ExpectedVersion);
