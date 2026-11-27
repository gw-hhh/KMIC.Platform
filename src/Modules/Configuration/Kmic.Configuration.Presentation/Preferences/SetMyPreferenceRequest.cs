using System.Text.Json;

namespace Kmic.Configuration.Presentation.Preferences;

/// <param name="Value">偏好值</param>
public sealed record SetMyPreferenceRequest(JsonElement Value);
