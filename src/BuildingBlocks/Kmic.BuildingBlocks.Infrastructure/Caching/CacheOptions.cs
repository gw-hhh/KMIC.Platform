namespace Kmic.BuildingBlocks.Infrastructure.Caching;

/// <summary>缓存配置</summary>
public sealed class CacheOptions
{
    public const string Section = "Kmic:Cache";

    /// <summary>本地缓存默认过期时间</summary>
    public TimeSpan DefaultLocalExpiration { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>单条失效广播最大标签字节数（须小于 PostgreSQL NOTIFY 的 8000 字节上限），超出时自动分批</summary>
    public int MaxNotifyBytes { get; set; } = 7000;
}
