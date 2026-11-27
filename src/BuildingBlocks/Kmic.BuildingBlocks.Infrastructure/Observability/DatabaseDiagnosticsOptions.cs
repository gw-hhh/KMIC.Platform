namespace Kmic.BuildingBlocks.Infrastructure.Observability;

/// <summary>数据库观测配置（慢查询日志与积压采样）</summary>
public sealed class DatabaseDiagnosticsOptions
{
    public const string Section = "Kmic:Observability";

    /// <summary>慢查询警告阈值（毫秒）</summary>
    public int SlowQueryMs { get; set; } = 500;

    /// <summary>是否在慢查询日志中输出参数化 SQL（含表名/列名，生产环境可关闭以仅记录语句类型）</summary>
    public bool RecordSqlText { get; set; } = true;

    /// <summary>积压量表采样间隔（超过三个间隔未更新时停止上报）</summary>
    public TimeSpan BacklogSampleInterval { get; set; } = TimeSpan.FromSeconds(30);
}
