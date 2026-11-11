using System.Text.Json;

namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

/// <summary>
/// 请求摘要与保存的响应值使用的 JSON 选项：属性按声明顺序输出，同一个请求的摘要稳定
/// </summary>
internal static class IdempotencyJson
{
    public static JsonSerializerOptions Options { get; } = new(JsonSerializerDefaults.Web);
}
