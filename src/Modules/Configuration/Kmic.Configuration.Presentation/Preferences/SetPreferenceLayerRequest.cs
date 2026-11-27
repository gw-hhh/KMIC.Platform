using System.Text.Json;

namespace Kmic.Configuration.Presentation.Preferences;

/// <param name="Value">偏好值</param>
/// <param name="ExpectedVersion">预期版本号，首次设置时为空</param>
public sealed record SetPreferenceLayerRequest(JsonElement Value, long? ExpectedVersion);
