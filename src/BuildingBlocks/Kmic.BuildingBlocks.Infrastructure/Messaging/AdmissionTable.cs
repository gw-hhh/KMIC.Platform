namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>容量保护共享状态表（单行，归属 Operations 模块，每周期由持锁实例写入、其余实例读取）</summary>
public static class AdmissionTable
{
    public const string Name = """
        "Operations"."MessagingAdmission"
        """;

    /// <summary>建表语句（由 Operations 迁移脚本与框架测试执行）</summary>
    public const string CreateSql = """
        CREATE TABLE "Operations"."MessagingAdmission" (
            "Id" smallint NOT NULL,
            "State" smallint NOT NULL,
            "LowWaterPeriods" integer NOT NULL,
            "DatabaseBytes" bigint NOT NULL,
            "UnfinishedBytes" bigint NOT NULL,
            "CapacityBytes" bigint NULL,
            "CapacityUpdatedAt" timestamptz NULL,
            "SampledAt" timestamptz NOT NULL,
            CONSTRAINT "PK_MessagingAdmission" PRIMARY KEY ("Id"),
            CONSTRAINT "CK_MessagingAdmission_SingleRow" CHECK ("Id" = 1));
        COMMENT ON TABLE "Operations"."MessagingAdmission" IS '消息写入的容量保护状态，只有一行：状态、已用空间、未完成消息量、容量和判断时间';
        """;

    // 咨询锁键：同一时刻仅允许单个实例执行采样
    internal const long LockKey = 0x4B4D4943_41444D31;
}
