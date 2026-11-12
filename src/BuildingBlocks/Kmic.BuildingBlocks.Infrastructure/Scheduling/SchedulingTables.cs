namespace Kmic.BuildingBlocks.Infrastructure.Scheduling;

/// <summary>
/// 定时任务用到的表
/// </summary>
/// <remarks>
/// 表都属于 Operations 模块，由它的迁移建立；调度器、同步和执行记录直接读写
/// </remarks>
public static class SchedulingTables
{
    /// <summary>
    /// Quartz 表名的前缀，带 schema
    /// </summary>
    public const string QuartzPrefix = """
        "Operations".qrtz_
        """;

    /// <summary>
    /// 定时任务设置表（管理员的开关与 Cron 覆盖），带 schema
    /// </summary>
    public const string Settings = """
        "Operations"."ScheduledJobSetting"
        """;

    /// <summary>
    /// 定时任务执行记录表，带 schema
    /// </summary>
    public const string Runs = """
        "Operations"."ScheduledJobRun"
        """;

    /// <summary>
    /// 设置表与执行记录表的建表语句：Operations 模块的迁移执行，框架测试直接执行
    /// </summary>
    public const string CreateSql = """
        CREATE TABLE "Operations"."ScheduledJobSetting" (
            "JobName" varchar(200) NOT NULL,
            "Enabled" boolean,
            "Cron" varchar(120),
            "UpdatedBy" uuid,
            "UpdatedAt" timestamptz NOT NULL DEFAULT clock_timestamp(),
            "ConcurrencyVersion" integer NOT NULL DEFAULT 0,
            CONSTRAINT "PK_ScheduledJobSetting" PRIMARY KEY ("JobName"));
        COMMENT ON TABLE "Operations"."ScheduledJobSetting" IS '定时任务的覆盖值：只存改过的项，没改的按代码里的默认值';
        CREATE TABLE "Operations"."ScheduledJobRun" (
            "Id" uuid NOT NULL,
            "JobName" varchar(200) NOT NULL,
            "ScheduledFireTime" timestamptz NOT NULL,
            "ActualFireTime" timestamptz NOT NULL,
            "Trigger" smallint NOT NULL,
            "TriggeredBy" uuid,
            "StartedAt" timestamptz NOT NULL DEFAULT clock_timestamp(),
            "FinishedAt" timestamptz,
            "DurationMs" bigint,
            "Status" smallint NOT NULL,
            "ItemsProcessed" integer,
            "Note" varchar(1000),
            "ErrorCode" varchar(200),
            "ErrorMessage" varchar(2000),
            "InstanceId" varchar(200) NOT NULL,
            "TraceId" varchar(64),
            CONSTRAINT "PK_ScheduledJobRun" PRIMARY KEY ("Id"));
        CREATE INDEX "IX_ScheduledJobRun_JobName_StartedAt" ON "Operations"."ScheduledJobRun" ("JobName", "StartedAt" DESC);
        CREATE INDEX "IX_ScheduledJobRun_Running" ON "Operations"."ScheduledJobRun" ("StartedAt") WHERE "Status" = 0;
        COMMENT ON TABLE "Operations"."ScheduledJobRun" IS '定时任务的执行记录';
        """;

    /// <summary>
    /// Quartz 自带的 PostgreSQL 建表语句，换上本系统的表名前缀
    /// </summary>
    /// <remarks>
    /// 语句都是 IF NOT EXISTS，重复执行没有影响。做 Operations 模块的迁移时要把这段 SQL 原样写进去，
    /// 不在迁移里读程序集资源：迁移必须固定不变，升级 Quartz 时另写迁移补列
    /// </remarks>
    public static string QuartzCreateSql()
    {
        using var stream = typeof(Quartz.IJob).Assembly.GetManifestResourceStream("Quartz.Impl.AdoJobStore.Schema.create_postgres.sql")
            ?? throw new InvalidOperationException("Quartz 程序集里找不到 PostgreSQL 建表语句。");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd()
            .Replace("--;;", "", StringComparison.Ordinal)
            .Replace("{0}", QuartzPrefix, StringComparison.Ordinal)
            .Replace("{1}", "qrtz_", StringComparison.Ordinal);
    }
}
