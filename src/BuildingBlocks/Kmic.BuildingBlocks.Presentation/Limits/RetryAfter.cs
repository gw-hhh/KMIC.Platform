using System.Globalization;

namespace Kmic.BuildingBlocks.Presentation.Limits;

/// <summary>
/// Retry-After 响应头
/// </summary>
internal static class RetryAfter
{
    // 暂时不可用或并发已满时，没有更准确的信息就给一个短的固定值，避免立即重试压垮刚恢复的依赖
    public const int DefaultSeconds = 5;

    public static void Set(HttpResponse response, int seconds = DefaultSeconds) =>
        response.Headers.RetryAfter = seconds.ToString(CultureInfo.InvariantCulture);
}
