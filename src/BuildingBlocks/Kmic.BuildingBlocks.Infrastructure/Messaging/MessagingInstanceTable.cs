namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 实例心跳表：每个运行中的实例一行，登记构建号和它声明的订阅
/// </summary>
/// <remarks>
/// 只用于诊断：运维终结订阅之前，据此看哪些构建还在声明它。不参与投递，投递只看路由表。
/// 表属于 Operations 模块，由它的迁移执行这里的建表语句
/// </remarks>
public static class MessagingInstanceTable
{
    /// <summary>
    /// 表名，带 schema
    /// </summary>
    public const string Name = """
        "Operations"."MessagingInstance"
        """;

    /// <summary>
    /// 建表语句：Operations 模块的迁移执行，框架测试直接执行
    /// </summary>
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

    /// <summary>
    /// 超过这么久没有心跳的行在下一次登记时顺手删掉
    /// </summary>
    internal static readonly TimeSpan Retention = TimeSpan.FromDays(1);
}
