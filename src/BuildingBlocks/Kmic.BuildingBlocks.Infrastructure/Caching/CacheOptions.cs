namespace Kmic.BuildingBlocks.Infrastructure.Caching;

/// <summary>
/// 缓存配置
/// </summary>
public sealed class CacheOptions
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public const string Section = "Kmic:Cache";

    /// <summary>
    /// 本地缓存的默认存活时间
    /// </summary>
    /// <remarks>
    /// 正常情况下缓存靠提交后失效来更新，这个时间只是兜底
    /// </remarks>
    public TimeSpan DefaultLocalExpiration { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// 一条失效广播最多带多少字节的标签，超过就分成几条
    /// </summary>
    /// <remarks>
    /// PostgreSQL 的通知内容上限是 8000 字节
    /// </remarks>
    public int MaxNotifyBytes { get; set; } = 7000;
}
