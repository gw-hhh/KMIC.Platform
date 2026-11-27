namespace Kmic.ServiceDefaults;

public sealed class ObservabilityOptions
{
    public const string Section = "Kmic:Observability";

    /// <summary>是否开启 HTTP 请求摘要日志</summary>
    public bool RequestLog { get; set; } = true;
}
