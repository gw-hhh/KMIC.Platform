namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>订阅路由表（归属 Operations 模块）</summary>
public static class MessageRouteTable
{
    public const string Name = """
        "Operations"."MessageRoute"
        """;

    /// <summary>建表语句（由 Operations 迁移脚本与框架测试执行）</summary>
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

    /// <summary>使用中</summary>
    public const short Active = 0;

    /// <summary>待终结（代码已移除订阅，继续投递并等待运维确认）</summary>
    public const short Retiring = 1;

    /// <summary>已终结（停止投递）</summary>
    public const short Finalized = 2;
}
