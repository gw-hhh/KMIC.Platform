namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 数据库配置
/// </summary>
/// <remarks>
/// 调整连接池时要保证：实例数 ×（Api 和 Worker 的事务池、读池上限之和 + 1 个 LISTEN 连接）+ Worker 副本数 × 流式池上限，
/// 不超过数据库的 max_connections 减去预留
/// </remarks>
public sealed class DatabaseOptions
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public const string Section = "Kmic:Database";

    /// <summary>
    /// 运行账号的连接串
    /// </summary>
    public string ConnectionString { get; set; } = "";

    /// <summary>
    /// 迁移账号的连接串，只用于开发环境的自动迁移
    /// </summary>
    /// <remarks>
    /// 不配置时用运行账号的连接串（本机开发库的账号通常什么权限都有）。生产环境的迁移由 Kmic.MigrationRunner 执行，应用里不配置迁移账号
    /// </remarks>
    public string MigrationConnectionString { get; set; } = "";

    /// <summary>
    /// 启动时是否自动执行迁移
    /// </summary>
    /// <remarks>
    /// 开发环境默认开启，其他环境默认关闭，只检查有没有未执行的迁移
    /// </remarks>
    public bool AutoMigrate { get; set; }

    /// <summary>
    /// 是否检查查询漏读未保存的修改
    /// </summary>
    /// <remarks>
    /// 每次查询多做一次变更检测，生产环境关闭
    /// </remarks>
    public bool CheckQueryVisibility { get; set; }

    /// <summary>
    /// HTTP 请求的连接池
    /// </summary>
    public DatabasePoolOptions Api { get; set; } = new() { TxMaxPoolSize = 30, ReadMaxPoolSize = 20 };

    /// <summary>
    /// 后台处理的连接池
    /// </summary>
    public DatabasePoolOptions Worker { get; set; } = new() { TxMaxPoolSize = 12, ReadMaxPoolSize = 8, StreamMaxPoolSize = 2 };
}
