namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 订阅路由表
/// </summary>
/// <remarks>
/// 表属于 Operations 模块，由它的迁移执行这里的建表语句；投递程序和启动登记直接读写
/// </remarks>
internal static class MessageRouteTable
{
    public const string Name = """
        "Operations"."MessageRoute"
        """;

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

    public const short Active = 0;

    public const short Retiring = 1;

    public const short Finalized = 2;
}
