namespace Kmic.BuildingBlocks.Infrastructure.SecurityEvents;

/// <summary>
/// 安全事件表
/// </summary>
/// <remarks>
/// 表属于 Identity 模块，由它的迁移执行这里的建表语句；框架的写入器直接插入。只追加：运行账号改不了、删不了
/// </remarks>
public static class SecurityEventTable
{
    /// <summary>
    /// 带 schema 的表名
    /// </summary>
    public const string Name = """
        "Identity"."SecurityEvent"
        """;

    /// <summary>
    /// 建表语句，含索引、表注释，并收回运行账号的修改与删除权限
    /// </summary>
    public const string CreateSql = """
        CREATE TABLE "Identity"."SecurityEvent" (
            "Id" uuid NOT NULL,
            "OccurredAt" timestamptz NOT NULL,
            "Type" varchar(100) NOT NULL,
            "Severity" smallint NOT NULL,
            "Reason" varchar(200),
            "UserId" uuid,
            "ActualUserId" uuid,
            "RoleId" uuid,
            "SessionId" uuid,
            "AttemptedAccount" varchar(100),
            "ClientIp" inet,
            "UserAgent" varchar(500),
            "ResourceType" varchar(200),
            "ResourceId" uuid,
            "Details" jsonb,
            "TraceId" varchar(32) NOT NULL,
            CONSTRAINT "PK_SecurityEvent" PRIMARY KEY ("Id"));
        CREATE INDEX "IX_SecurityEvent_UserId_OccurredAt" ON "Identity"."SecurityEvent" ("UserId", "OccurredAt" DESC);
        CREATE INDEX "IX_SecurityEvent_ClientIp_OccurredAt" ON "Identity"."SecurityEvent" ("ClientIp", "OccurredAt" DESC);
        CREATE INDEX "IX_SecurityEvent_Type_OccurredAt" ON "Identity"."SecurityEvent" ("Type", "OccurredAt" DESC);
        COMMENT ON TABLE "Identity"."SecurityEvent" IS '安全事件：登录失败、越权访问、授权配置变更，只追加';
        REVOKE UPDATE, DELETE ON TABLE "Identity"."SecurityEvent" FROM kmic_runtime;
        """;
}
