namespace Kmic.BuildingBlocks.Infrastructure.Observability;

/// <summary>
/// 数据库侧的观测配置：慢查询日志与积压采样
/// </summary>
public sealed class DatabaseDiagnosticsOptions
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public const string Section = "Kmic:Observability";

    /// <summary>
    /// 慢查询阈值，单位毫秒，单条 SQL 达到这个时长就记一条警告
    /// </summary>
    public int SlowQueryMs { get; set; } = 500;

    /// <summary>
    /// 慢查询日志里是否带参数化的 SQL 文本，关闭后只记语句类型（SELECT、UPDATE 等）
    /// </summary>
    /// <remarks>
    /// 文本里没有参数值，只有占位符；不过表名、列名也算内部信息，生产环境按需要关闭
    /// </remarks>
    public bool RecordSqlText { get; set; } = true;

    /// <summary>
    /// 积压量表的采样间隔。采样数据超过三个间隔没有更新，就不再输出，不输出过期的数字
    /// </summary>
    public TimeSpan BacklogSampleInterval { get; set; } = TimeSpan.FromSeconds(30);
}
