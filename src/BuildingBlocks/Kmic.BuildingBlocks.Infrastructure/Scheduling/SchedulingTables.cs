namespace Kmic.BuildingBlocks.Infrastructure.Scheduling;

/// <summary>定时任务相关表定义（归属 Operations 模块，由其迁移创建）</summary>
public static class SchedulingTables
{
    public const string QuartzPrefix = """
        "Operations".qrtz_
        """;

    /// <summary>运维覆盖配置表名（含 schema），存储管理员调整的开关与 Cron</summary>
    public const string Settings = """
        "Operations"."ScheduledJobSetting"
        """;

    public const string Runs = """
        "Operations"."ScheduledJobRun"
        """;

    /// <summary>配置表与执行记录表建表 DDL（供 Operations 迁移及框架测试使用）</summary>
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

    /// <summary>生成替换表前缀后的 Quartz PostgreSQL 建表脚本（迁移中应直接内联 SQL）</summary>
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
