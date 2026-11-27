using System.Text.Json;

namespace Kmic.Configuration.Presentation.Settings;

/// <param name="Value">新的设置值</param>
/// <param name="Reason">原因</param>
/// <param name="ExpectedVersion">预期版本号，首次设置时为空</param>
public sealed record SetSettingRequest(JsonElement Value, string Reason, long? ExpectedVersion);
