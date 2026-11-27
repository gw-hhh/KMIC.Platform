using System.Globalization;

namespace Kmic.BuildingBlocks.Presentation.Limits;

internal static class RetryAfter
{
    // 服务暂时不可用或并发已满且无精确等待时间时，返回较短的固定退避秒数，避免客户端立即重试压垮刚恢复的依赖
    public const int DefaultSeconds = 5;

    public static void Set(HttpResponse response, int seconds = DefaultSeconds) =>
        response.Headers.RetryAfter = seconds.ToString(CultureInfo.InvariantCulture);
}
