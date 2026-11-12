using System;
using System.Net;
using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Infrastructure.Scheduling;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Kmic.Operations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitializeModuleSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InitializeModuleSchema("Operations");

            migrationBuilder.EnsureSchema(
                name: "Operations");

            migrationBuilder.CreateTable(
                name: "AuditLog",
                schema: "Operations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID"),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "操作发生的时间"),
                    Module = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "产生这条审计的模块"),
                    Action = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "操作名，如 Identity.CreateUser"),
                    Origin = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, comment: "请求来源，如 Http、ScheduledJob"),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true, comment: "操作人；系统身份与匿名请求没有"),
                    ActualUserId = table.Column<Guid>(type: "uuid", nullable: true, comment: "模拟登录时真正动手的人"),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: true, comment: "操作时使用的角色"),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: true, comment: "会话"),
                    OnBehalfOfUserId = table.Column<Guid>(type: "uuid", nullable: true, comment: "发起原始请求的用户：事件处理器代表他执行时记录"),
                    ClientIp = table.Column<IPAddress>(type: "inet", nullable: true, comment: "客户端 IP"),
                    TraceId = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, comment: "链路追踪 ID，与错误响应和日志中的一致"),
                    Targets = table.Column<string>(type: "jsonb", nullable: false, comment: "操作对象，JSON 数组：资源类型与 ID"),
                    Changes = table.Column<string>(type: "jsonb", nullable: false, comment: "字段变更，JSON 数组：实体、变更类型、各字段新旧值（敏感字段已脱敏）"),
                    Details = table.Column<string>(type: "jsonb", nullable: false, comment: "补充说明，JSON 对象，如导出的筛选条件"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "创建人"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "修改时间"),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "修改人"),
                    ConcurrencyVersion = table.Column<long>(type: "bigint", nullable: false, comment: "并发版本号")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLog", x => x.Id);
                },
                comment: "审计汇总；Operations 模块；订阅各模块的审计消息按审计 ID 幂等写入，只追加，运行账号没有修改与删除权限；创建时间是汇总到这里的时间");

            migrationBuilder.CreateTable(
                name: "BackgroundJob",
                schema: "Operations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SchemaVersion = table.Column<int>(type: "integer", nullable: false),
                    Args = table.Column<string>(type: "jsonb", nullable: false),
                    SubmittedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedRoleId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActualUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    ProgressDone = table.Column<long>(type: "bigint", nullable: false),
                    ProgressTotal = table.Column<long>(type: "bigint", nullable: true),
                    ProgressMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Checkpoint = table.Column<string>(type: "jsonb", nullable: true),
                    ResultFileId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResultSummary = table.Column<string>(type: "jsonb", nullable: true),
                    ErrorCode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Attempts = table.Column<int>(type: "integer", nullable: false),
                    LeaseToken = table.Column<Guid>(type: "uuid", nullable: true),
                    LeaseUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FinishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelRequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelRequestedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackgroundJob", x => x.Id);
                },
                comment: "后台作业");

            migrationBuilder.CreateTable(
                name: "IdempotencyRecord",
                schema: "Operations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Operation = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RequestHash = table.Column<byte[]>(type: "bytea", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    LeaseToken = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaseExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ResponseBody = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdempotencyRecord", x => x.Id);
                },
                comment: "幂等记录");

            migrationBuilder.CreateTable(
                name: "InboxMessage",
                schema: "Operations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Subscription = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MessageType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SchemaVersion = table.Column<int>(type: "integer", nullable: false),
                    Payload = table.Column<string>(type: "jsonb", nullable: false),
                    Headers = table.Column<string>(type: "jsonb", nullable: false),
                    SourceModule = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceVersion = table.Column<long>(type: "bigint", nullable: true),
                    OrderingKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Seq = table.Column<long>(type: "bigint", nullable: false),
                    SubjectIds = table.Column<Guid[]>(type: "uuid[]", nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "clock_timestamp()"),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    Attempts = table.Column<int>(type: "integer", nullable: false),
                    NextAttemptAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "clock_timestamp()"),
                    LeaseToken = table.Column<Guid>(type: "uuid", nullable: true),
                    LeaseUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxMessage", x => x.Id);
                },
                comment: "待处理的集成消息，每个订阅一行");

            migrationBuilder.CreateTable(
                name: "NumberCounter",
                schema: "Operations",
                columns: table => new
                {
                    Rule = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PeriodKey = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    PartitionKey = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    LastValue = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NumberCounter", x => new { x.Rule, x.PeriodKey, x.PartitionKey });
                },
                comment: "业务编号的计数");

            migrationBuilder.CreateTable(
                name: "OutboxMessage",
                schema: "Operations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Seq = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    MessageType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SchemaVersion = table.Column<int>(type: "integer", nullable: false),
                    Kind = table.Column<short>(type: "smallint", nullable: false),
                    Payload = table.Column<string>(type: "jsonb", nullable: false),
                    Headers = table.Column<string>(type: "jsonb", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceVersion = table.Column<long>(type: "bigint", nullable: true),
                    OrderingKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SubjectIds = table.Column<Guid[]>(type: "uuid[]", nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    Attempts = table.Column<int>(type: "integer", nullable: false),
                    NextAttemptAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "clock_timestamp()"),
                    LeaseToken = table.Column<Guid>(type: "uuid", nullable: true),
                    LeaseUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    DeliveredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessage", x => x.Id);
                },
                comment: "待投递的集成消息");

            migrationBuilder.CreateTable(
                name: "ReadModelRebuildRun",
                schema: "Operations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Subscription = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ReadModel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    ScannedCount = table.Column<long>(type: "bigint", nullable: false),
                    MissingCount = table.Column<long>(type: "bigint", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "clock_timestamp()"),
                    FinishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReadModelRebuildRun", x => x.Id);
                },
                comment: "读模型的重建轮次");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_Action_OccurredAt",
                schema: "Operations",
                table: "AuditLog",
                columns: new[] { "Action", "OccurredAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_ActorUserId_OccurredAt",
                schema: "Operations",
                table: "AuditLog",
                columns: new[] { "ActorUserId", "OccurredAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_CreatedAt",
                schema: "Operations",
                table: "AuditLog",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_OccurredAt",
                schema: "Operations",
                table: "AuditLog",
                column: "OccurredAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_Targets",
                schema: "Operations",
                table: "AuditLog",
                column: "Targets")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "jsonb_path_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundJob_ExpiresAt",
                schema: "Operations",
                table: "BackgroundJob",
                column: "ExpiresAt",
                filter: "\"ExpiresAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundJob_LeaseUntil",
                schema: "Operations",
                table: "BackgroundJob",
                column: "LeaseUntil",
                filter: "\"Status\" IN (0, 1, 4)");

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundJob_SubmittedBy_SubmittedAt",
                schema: "Operations",
                table: "BackgroundJob",
                columns: new[] { "SubmittedBy", "SubmittedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_IdempotencyRecord_ExpiresAt",
                schema: "Operations",
                table: "IdempotencyRecord",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_IdempotencyRecord_UserId_RoleId_Operation_Key",
                schema: "Operations",
                table: "IdempotencyRecord",
                columns: new[] { "UserId", "RoleId", "Operation", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessage_MessageId_Subscription",
                schema: "Operations",
                table: "InboxMessage",
                columns: new[] { "MessageId", "Subscription" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessage_NextAttemptAt",
                schema: "Operations",
                table: "InboxMessage",
                column: "NextAttemptAt",
                filter: "\"Status\" IN (0, 1)");

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessage_ReceivedAt",
                schema: "Operations",
                table: "InboxMessage",
                column: "ReceivedAt",
                filter: "\"Status\" = 2");

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessage_SubjectIds",
                schema: "Operations",
                table: "InboxMessage",
                column: "SubjectIds",
                filter: "\"SubjectIds\" IS NOT NULL AND \"Status\" IN (0, 1, 3)")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessage_Subscription",
                schema: "Operations",
                table: "InboxMessage",
                column: "Subscription",
                filter: "\"Status\" IN (0, 1, 3)");

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessage_Subscription_OrderingKey_Seq",
                schema: "Operations",
                table: "InboxMessage",
                columns: new[] { "Subscription", "OrderingKey", "Seq" },
                filter: "\"Status\" IN (0, 1, 3) AND \"OrderingKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_NextAttemptAt",
                schema: "Operations",
                table: "OutboxMessage",
                column: "NextAttemptAt",
                filter: "\"Status\" IN (0, 1)");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_Status_OccurredAt",
                schema: "Operations",
                table: "OutboxMessage",
                columns: new[] { "Status", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_SubjectIds",
                schema: "Operations",
                table: "OutboxMessage",
                column: "SubjectIds",
                filter: "\"SubjectIds\" IS NOT NULL AND \"Status\" IN (0, 1, 3)")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_ReadModelRebuildRun_Subscription",
                schema: "Operations",
                table: "ReadModelRebuildRun",
                column: "Subscription",
                unique: true,
                filter: "\"Status\" IN (0, 1)");

            // 消息路由表、定时任务的设置与执行记录表、Quartz 的集群表属于本模块，建表语句由框架提供
            migrationBuilder.Sql(MessageRouteTable.CreateSql);
            migrationBuilder.Sql(SchedulingTables.CreateSql);
            migrationBuilder.Sql(SchedulingTables.QuartzCreateSql());
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"DROP TABLE IF EXISTS {MessageRouteTable.Name};");
            migrationBuilder.Sql($"DROP TABLE IF EXISTS {SchedulingTables.Settings};");
            migrationBuilder.Sql($"DROP TABLE IF EXISTS {SchedulingTables.Runs};");
            migrationBuilder.Sql("""
                DO $$
                DECLARE r record;
                BEGIN
                    FOR r IN SELECT tablename FROM pg_tables WHERE schemaname = 'Operations' AND tablename LIKE 'qrtz\_%' LOOP
                        EXECUTE format('DROP TABLE IF EXISTS "Operations".%I CASCADE', r.tablename);
                    END LOOP;
                END $$;
                """);

            migrationBuilder.DropTable(
                name: "AuditLog",
                schema: "Operations");

            migrationBuilder.DropTable(
                name: "BackgroundJob",
                schema: "Operations");

            migrationBuilder.DropTable(
                name: "IdempotencyRecord",
                schema: "Operations");

            migrationBuilder.DropTable(
                name: "InboxMessage",
                schema: "Operations");

            migrationBuilder.DropTable(
                name: "NumberCounter",
                schema: "Operations");

            migrationBuilder.DropTable(
                name: "OutboxMessage",
                schema: "Operations");

            migrationBuilder.DropTable(
                name: "ReadModelRebuildRun",
                schema: "Operations");
        }
    }
}
