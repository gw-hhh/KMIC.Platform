namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>数据库配置（连接池须满足：实例数 × (事务池 + 读池上限 + 1) + Worker 副本数 × 流式池上限 ≤ max_connections − 预留连接数）</summary>
public sealed class DatabaseOptions
{
    public const string Section = "Kmic:Database";

    /// <summary>运行账号连接串</summary>
    public string ConnectionString
    {
        get;
        set => field = KmicConnectionString.Normalize(value);
    } = "";

    /// <summary>迁移账号连接串（仅用于开发环境自动迁移，未配置时回退至运行账号；生产迁移由 Kmic.MigrationRunner 执行）</summary>
    public string MigrationConnectionString
    {
        get;
        set => field = KmicConnectionString.Normalize(value);
    } = "";

    /// <summary>启动时是否自动执行迁移（开发环境默认开启）</summary>
    public bool AutoMigrate { get; set; }

    /// <summary>是否检测查询漏读当前事务中未保存的修改（启用后每次查询会额外执行变更检测，生产环境应关闭）</summary>
    public bool CheckQueryVisibility { get; set; }

    /// <summary>HTTP 请求连接池配置</summary>
    public DatabasePoolOptions Api { get; set; } = new() { TxMaxPoolSize = 30, ReadMaxPoolSize = 20 };

    /// <summary>后台处理连接池配置</summary>
    public DatabasePoolOptions Worker { get; set; } = new() { TxMaxPoolSize = 12, ReadMaxPoolSize = 8, StreamMaxPoolSize = 2 };
}
