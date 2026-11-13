using System;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Kmic.Files.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitializeModuleSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InitializeModuleSchema("Files");

            migrationBuilder.EnsureSchema(
                name: "Files");

            migrationBuilder.CreateTable(
                name: "BackgroundJob",
                schema: "Files",
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
                schema: "Files",
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
                schema: "Files",
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
                schema: "Files",
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
                schema: "Files",
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
                schema: "Files",
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
                name: "IX_BackgroundJob_ExpiresAt",
                schema: "Files",
                table: "BackgroundJob",
                column: "ExpiresAt",
                filter: "\"ExpiresAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundJob_LeaseUntil",
                schema: "Files",
                table: "BackgroundJob",
                column: "LeaseUntil",
                filter: "\"Status\" IN (0, 1, 4)");

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundJob_SubmittedBy_SubmittedAt",
                schema: "Files",
                table: "BackgroundJob",
                columns: new[] { "SubmittedBy", "SubmittedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_IdempotencyRecord_ExpiresAt",
                schema: "Files",
                table: "IdempotencyRecord",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_IdempotencyRecord_UserId_RoleId_Operation_Key",
                schema: "Files",
                table: "IdempotencyRecord",
                columns: new[] { "UserId", "RoleId", "Operation", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessage_MessageId_Subscription",
                schema: "Files",
                table: "InboxMessage",
                columns: new[] { "MessageId", "Subscription" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessage_NextAttemptAt",
                schema: "Files",
                table: "InboxMessage",
                column: "NextAttemptAt",
                filter: "\"Status\" IN (0, 1)");

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessage_ReceivedAt",
                schema: "Files",
                table: "InboxMessage",
                column: "ReceivedAt",
                filter: "\"Status\" = 2");

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessage_SubjectIds",
                schema: "Files",
                table: "InboxMessage",
                column: "SubjectIds",
                filter: "\"SubjectIds\" IS NOT NULL AND \"Status\" IN (0, 1, 3)")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessage_Subscription",
                schema: "Files",
                table: "InboxMessage",
                column: "Subscription",
                filter: "\"Status\" IN (0, 1, 3)");

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessage_Subscription_OrderingKey_Seq",
                schema: "Files",
                table: "InboxMessage",
                columns: new[] { "Subscription", "OrderingKey", "Seq" },
                filter: "\"Status\" IN (0, 1, 3) AND \"OrderingKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_NextAttemptAt",
                schema: "Files",
                table: "OutboxMessage",
                column: "NextAttemptAt",
                filter: "\"Status\" IN (0, 1)");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_Status_OccurredAt",
                schema: "Files",
                table: "OutboxMessage",
                columns: new[] { "Status", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_SubjectIds",
                schema: "Files",
                table: "OutboxMessage",
                column: "SubjectIds",
                filter: "\"SubjectIds\" IS NOT NULL AND \"Status\" IN (0, 1, 3)")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_ReadModelRebuildRun_Subscription",
                schema: "Files",
                table: "ReadModelRebuildRun",
                column: "Subscription",
                unique: true,
                filter: "\"Status\" IN (0, 1)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BackgroundJob",
                schema: "Files");

            migrationBuilder.DropTable(
                name: "IdempotencyRecord",
                schema: "Files");

            migrationBuilder.DropTable(
                name: "InboxMessage",
                schema: "Files");

            migrationBuilder.DropTable(
                name: "NumberCounter",
                schema: "Files");

            migrationBuilder.DropTable(
                name: "OutboxMessage",
                schema: "Files");

            migrationBuilder.DropTable(
                name: "ReadModelRebuildRun",
                schema: "Files");
        }
    }
}
