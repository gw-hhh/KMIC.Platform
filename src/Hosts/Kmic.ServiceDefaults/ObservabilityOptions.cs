namespace Kmic.ServiceDefaults;

/// <summary>
/// 可观测性配置
/// </summary>
public sealed class ObservabilityOptions
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public const string Section = "Kmic:Observability";

    /// <summary>
    /// 是否启用请求日志
    /// </summary>
    public bool RequestLog { get; set; } = true;
}
