using System.Text.Json;

namespace Kmic.Configuration.Presentation.Settings;

/// <summary>
/// 修改系统设置的请求
/// </summary>
/// <param name="Value">新的设置值</param>
/// <param name="Reason">原因</param>
/// <param name="ExpectedVersion">预期版本号，还没有值时不传</param>
public sealed record SetSettingRequest(JsonElement Value, string Reason, long? ExpectedVersion);
