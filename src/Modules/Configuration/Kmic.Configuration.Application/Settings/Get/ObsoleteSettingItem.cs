using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kmic.Configuration.Application.Settings.Get;

/// <summary>
/// 代码里已经不存在的设置留下的值
/// </summary>
/// <param name="Key">设置键</param>
/// <param name="Value">留下的值</param>
/// <param name="ConcurrencyVersion">版本号，输出为 version；清除时带上它</param>
public sealed record ObsoleteSettingItem(string Key, JsonElement Value, [property: JsonPropertyName("version")] long ConcurrencyVersion);
