using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kmic.Configuration.Application.Settings.Get;

/// <summary>代码中已移除设置的残留值</summary>
public sealed record ObsoleteSettingItem(string Key, JsonElement Value, [property: JsonPropertyName("version")] long ConcurrencyVersion);
