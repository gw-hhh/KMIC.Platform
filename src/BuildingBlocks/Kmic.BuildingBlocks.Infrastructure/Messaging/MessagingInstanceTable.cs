namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>实例心跳表（归属 Operations 模块，仅用于诊断）</summary>
public static class MessagingInstanceTable
{
    public const string Name = """
        "Operations"."MessagingInstance"
        """;

    /// <summary>建表语句（由 Operations 迁移脚本与框架测试执行）</summary>
    public const string CreateSql = """
        CREATE TABLE "Operations"."MessagingInstance" (
            "Id" uuid NOT NULL,
            "Host" varchar(200) NOT NULL,
            "Roles" varchar(50) NOT NULL,
            "Build" varchar(100) NOT NULL,
            "Subscriptions" jsonb NOT NULL,
            "StartedAt" timestamptz NOT NULL,
            "HeartbeatAt" timestamptz NOT NULL,
            CONSTRAINT "PK_MessagingInstance" PRIMARY KEY ("Id"));
        COMMENT ON TABLE "Operations"."MessagingInstance" IS '运行中的实例：构建号、角色、声明的订阅和最近一次心跳，只用于诊断';
        """;

    // 超时未上报的心跳记录在下次登记时一并清理
    internal static readonly TimeSpan Retention = TimeSpan.FromDays(1);
}
