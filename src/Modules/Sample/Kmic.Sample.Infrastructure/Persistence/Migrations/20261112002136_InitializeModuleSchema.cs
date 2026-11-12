using System;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Kmic.Sample.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitializeModuleSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InitializeModuleSchema("Sample");

            migrationBuilder.EnsureSchema(
                name: "Sample");

            migrationBuilder.CreateTable(
                name: "BackgroundJob",
                schema: "Sample",
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
                schema: "Sample",
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
                schema: "Sample",
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
                schema: "Sample",
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
                schema: "Sample",
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
                schema: "Sample",
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

            migrationBuilder.CreateTable(
                name: "Specimen",
                schema: "Sample",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID"),
                    Number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "编号，创建时由编号规则生成", collation: "CaseInsensitive"),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "标题"),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "状态"),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false, comment: "负责人，创建者"),
                    OrgUnitId = table.Column<Guid>(type: "uuid", nullable: false, comment: "所属组织，数据范围按它判断"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "创建人"),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "删除时间"),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "删除人"),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false, comment: "是否已删除"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "修改时间"),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "修改人"),
                    ConcurrencyVersion = table.Column<long>(type: "bigint", nullable: false, comment: "并发版本号")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specimen", x => x.Id);
                    table.CheckConstraint("CK_Specimen_Status", "\"Status\" IN ('Draft', 'Submitted', 'Closed')");
                },
                comment: "示例标本；Sample 模块；一行代表一个标本；可软删除，有状态流转");

            migrationBuilder.CreateTable(
                name: "SpecimenStateHistory",
                schema: "Sample",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AggregateId = table.Column<Guid>(type: "uuid", nullable: false),
                    StateName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FromCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ToCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ActionCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Revision = table.Column<int>(type: "integer", nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActualUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: true),
                    OnBehalfOfUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TraceId = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecimenStateHistory", x => x.Id);
                },
                comment: "Specimen 的状态流转记录，只追加");

            migrationBuilder.CreateTable(
                name: "SpecimenItem",
                schema: "Sample",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID"),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "名称"),
                    Quantity = table.Column<int>(type: "integer", nullable: false, comment: "数量"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "创建人"),
                    SpecimenId = table.Column<Guid>(type: "uuid", nullable: false, comment: "Specimen ID"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "修改时间"),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "修改人")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecimenItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpecimenItem_Specimen_SpecimenId",
                        column: x => x.SpecimenId,
                        principalSchema: "Sample",
                        principalTable: "Specimen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "示例标本的明细；Sample 模块；一行代表一项明细，随标本一起保存");

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundJob_ExpiresAt",
                schema: "Sample",
                table: "BackgroundJob",
                column: "ExpiresAt",
                filter: "\"ExpiresAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundJob_LeaseUntil",
                schema: "Sample",
                table: "BackgroundJob",
                column: "LeaseUntil",
                filter: "\"Status\" IN (0, 1, 4)");

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundJob_SubmittedBy_SubmittedAt",
                schema: "Sample",
                table: "BackgroundJob",
                columns: new[] { "SubmittedBy", "SubmittedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_IdempotencyRecord_ExpiresAt",
                schema: "Sample",
                table: "IdempotencyRecord",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_IdempotencyRecord_UserId_RoleId_Operation_Key",
                schema: "Sample",
                table: "IdempotencyRecord",
                columns: new[] { "UserId", "RoleId", "Operation", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessage_MessageId_Subscription",
                schema: "Sample",
                table: "InboxMessage",
                columns: new[] { "MessageId", "Subscription" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessage_NextAttemptAt",
                schema: "Sample",
                table: "InboxMessage",
                column: "NextAttemptAt",
                filter: "\"Status\" IN (0, 1)");

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessage_ReceivedAt",
                schema: "Sample",
                table: "InboxMessage",
                column: "ReceivedAt",
                filter: "\"Status\" = 2");

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessage_SubjectIds",
                schema: "Sample",
                table: "InboxMessage",
                column: "SubjectIds",
                filter: "\"SubjectIds\" IS NOT NULL AND \"Status\" IN (0, 1, 3)")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessage_Subscription_OrderingKey_Seq",
                schema: "Sample",
                table: "InboxMessage",
                columns: new[] { "Subscription", "OrderingKey", "Seq" },
                filter: "\"Status\" IN (0, 1, 3) AND \"OrderingKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_NextAttemptAt",
                schema: "Sample",
                table: "OutboxMessage",
                column: "NextAttemptAt",
                filter: "\"Status\" IN (0, 1)");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_Status_OccurredAt",
                schema: "Sample",
                table: "OutboxMessage",
                columns: new[] { "Status", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_SubjectIds",
                schema: "Sample",
                table: "OutboxMessage",
                column: "SubjectIds",
                filter: "\"SubjectIds\" IS NOT NULL AND \"Status\" IN (0, 1, 3)")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_ReadModelRebuildRun_Subscription",
                schema: "Sample",
                table: "ReadModelRebuildRun",
                column: "Subscription",
                unique: true,
                filter: "\"Status\" IN (0, 1)");

            migrationBuilder.CreateIndex(
                name: "IX_Specimen_CreatedAt",
                schema: "Sample",
                table: "Specimen",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Specimen_Number",
                schema: "Sample",
                table: "Specimen",
                column: "Number",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_SpecimenItem_SpecimenId",
                schema: "Sample",
                table: "SpecimenItem",
                column: "SpecimenId");

            migrationBuilder.CreateIndex(
                name: "IX_SpecimenStateHistory_AggregateId_OccurredAt",
                schema: "Sample",
                table: "SpecimenStateHistory",
                columns: new[] { "AggregateId", "OccurredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BackgroundJob",
                schema: "Sample");

            migrationBuilder.DropTable(
                name: "IdempotencyRecord",
                schema: "Sample");

            migrationBuilder.DropTable(
                name: "InboxMessage",
                schema: "Sample");

            migrationBuilder.DropTable(
                name: "NumberCounter",
                schema: "Sample");

            migrationBuilder.DropTable(
                name: "OutboxMessage",
                schema: "Sample");

            migrationBuilder.DropTable(
                name: "ReadModelRebuildRun",
                schema: "Sample");

            migrationBuilder.DropTable(
                name: "SpecimenItem",
                schema: "Sample");

            migrationBuilder.DropTable(
                name: "SpecimenStateHistory",
                schema: "Sample");

            migrationBuilder.DropTable(
                name: "Specimen",
                schema: "Sample");
        }
    }
}
