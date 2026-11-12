namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 订阅路由表
/// </summary>
/// <remarks>
/// 表属于 Operations 模块，由它的迁移执行这里的建表语句；投递程序和启动登记直接读写
/// </remarks>
public static class MessageRouteTable
{
    /// <summary>
    /// 表名，带 schema
    /// </summary>
    public const string Name = """
        "Operations"."MessageRoute"
        """;

    /// <summary>
    /// 建表语句：Operations 模块的迁移执行，框架测试直接执行
    /// </summary>
    public const string CreateSql = """
        CREATE TABLE "Operations"."MessageRoute" (
            "MessageType" varchar(200) NOT NULL,
            "Subscription" varchar(200) NOT NULL,
            "Module" varchar(100) NOT NULL,
            "Status" smallint NOT NULL DEFAULT 0,
            "RegisteredBuild" varchar(100) NOT NULL,
            "RegisteredAt" timestamptz NOT NULL DEFAULT clock_timestamp(),
            "NeedsInitialization" boolean NOT NULL DEFAULT false,
            "DisplayName" varchar(200),
            "RetiredReason" varchar(1000),
            "FinalizedAt" timestamptz,
            "FinalizedBy" uuid,
            CONSTRAINT "PK_MessageRoute" PRIMARY KEY ("MessageType", "Subscription"));
        COMMENT ON TABLE "Operations"."MessageRoute" IS '订阅路由：哪种消息投递给哪个模块的哪个订阅';
        """;

    /// <summary>
    /// 状态：使用中
    /// </summary>
    public const short Active = 0;

    /// <summary>
    /// 状态：待终结，订阅已从代码里去掉，消息照常投递并积压，等运维确认
    /// </summary>
    public const short Retiring = 1;

    /// <summary>
    /// 状态：已终结，不再投递
    /// </summary>
    public const short Finalized = 2;
}
