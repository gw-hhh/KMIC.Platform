namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 运行账号的数据库连接与连接池上限（配置节 Kmic:Database）
/// </summary>
/// <remarks>
/// 连接按来源分前台、后台，再分事务池与读池；Worker 另有导出用的流式池。
/// 连接预算：实例数 × (前台 + 后台 + 1 个监听连接) + Worker 副本数 × 流式池上限 ≤ max_connections − 预留
/// </remarks>
public sealed class DatabaseOptions
{
    /// <summary>
    /// 配置节
    /// </summary>
    public const string Section = "Kmic:Database";

    /// <summary>
    /// 运行账号的连接串，取自 ConnectionStrings:Kmic；密码只能来自环境变量或密钥存储，不写进配置文件
    /// </summary>
    public string ConnectionString { get; set; } = "";

    /// <summary>
    /// 是否检查“命令修改后再查询本模块”：开发与测试环境打开，生产环境关闭（每次查询多一次变更检测）
    /// </summary>
    public bool CheckQueryVisibility { get; set; }

    /// <summary>
    /// HTTP 请求使用的连接池，默认合计 50
    /// </summary>
    public DatabasePoolOptions Api { get; set; } = new() { TxMaxPoolSize = 30, ReadMaxPoolSize = 20 };

    /// <summary>
    /// 事件处理、定时任务、后台作业与启动任务使用的连接池，默认合计 20，流式池与作业并行数相同
    /// </summary>
    public DatabasePoolOptions Worker { get; set; } = new() { TxMaxPoolSize = 12, ReadMaxPoolSize = 8, StreamMaxPoolSize = 2 };
}
