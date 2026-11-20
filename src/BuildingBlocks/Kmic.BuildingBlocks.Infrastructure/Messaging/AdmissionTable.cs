namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 容量保护的共享状态表，只有一行
/// </summary>
/// <remarks>
/// 表属于 Operations 模块，由它的迁移执行这里的建表语句。每个周期由取到咨询锁的实例写入，其他实例读取
/// </remarks>
public static class AdmissionTable
{
    /// <summary>
    /// 表名，带 schema
    /// </summary>
    public const string Name = """
        "Operations"."MessagingAdmission"
        """;

    /// <summary>
    /// 建表语句：Operations 模块的迁移执行，框架测试直接执行
    /// </summary>
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

    /// <summary>
    /// 取咨询锁的键，同一时刻只有一个实例做判断
    /// </summary>
    internal const long LockKey = 0x4B4D4943_41444D31;
}
