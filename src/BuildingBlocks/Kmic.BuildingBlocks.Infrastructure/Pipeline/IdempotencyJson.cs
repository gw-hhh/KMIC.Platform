using System.Text.Json;

namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

/// <summary>
/// 幂等用的 JSON 选项
/// </summary>
/// <remarks>
/// 属性按声明顺序输出，同一个请求每次算出的摘要才一样
/// </remarks>
internal static class IdempotencyJson
{
    public static JsonSerializerOptions Options { get; } = new(JsonSerializerDefaults.Web);
}
