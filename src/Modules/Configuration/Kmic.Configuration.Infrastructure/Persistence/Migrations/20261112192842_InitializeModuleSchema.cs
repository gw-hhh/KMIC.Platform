using System;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Kmic.Configuration.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitializeModuleSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InitializeModuleSchema("Configuration");

            migrationBuilder.EnsureSchema(
                name: "Configuration");

            migrationBuilder.CreateTable(
                name: "BackgroundJob",
                schema: "Configuration",
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
                name: "DictionaryType",
                schema: "Configuration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID"),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "类型编码，以声明它的模块名开头，如 Sample.Industry"),
                    OwnerModule = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "声明它的模块"),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "显示名，来自代码声明，每次同步更新"),
                    IsHierarchical = table.Column<bool>(type: "boolean", nullable: false, comment: "是否层级字典"),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "状态"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "创建人"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "修改时间"),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "修改人"),
                    ConcurrencyVersion = table.Column<long>(type: "bigint", nullable: false, comment: "并发版本号")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DictionaryType", x => x.Id);
                    table.CheckConstraint("CK_DictionaryType_Status", "\"Status\" IN ('Active', 'Retired')");
                },
                comment: "字典类型；Configuration 模块；由代码声明、启动时同步；字典项是它的子实体，任何字典项的变化都递增它的版本号");

            migrationBuilder.CreateTable(
                name: "IdempotencyRecord",
                schema: "Configuration",
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
                schema: "Configuration",
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
                schema: "Configuration",
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
                schema: "Configuration",
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
                name: "PreferenceValue",
                schema: "Configuration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID"),
                    Key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "偏好键"),
                    Layer = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "所在的层"),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: true, comment: "角色层和用户层所属的角色；系统层为空"),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true, comment: "用户层所属的用户；系统层和角色层为空"),
                    Value = table.Column<string>(type: "jsonb", nullable: false, comment: "值，JSON 对象"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "创建人"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "修改时间"),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "修改人"),
                    ConcurrencyVersion = table.Column<long>(type: "bigint", nullable: false, comment: "并发版本号")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreferenceValue", x => x.Id);
                    table.CheckConstraint("CK_PreferenceValue_Layer", "\"Layer\" IN ('System', 'Role', 'UserRole')");
                    table.CheckConstraint("CK_PreferenceValue_LayerOwner", "(\"Layer\" = 'System' AND \"RoleId\" IS NULL AND \"UserId\" IS NULL)\nOR (\"Layer\" = 'Role' AND \"RoleId\" IS NOT NULL AND \"UserId\" IS NULL)\nOR (\"Layer\" = 'UserRole' AND \"RoleId\" IS NOT NULL AND \"UserId\" IS NOT NULL)");
                },
                comment: "偏好在某一层上的值；Configuration 模块；系统层、角色层、用户在角色下的层，各层同一个键只有一行");

            migrationBuilder.CreateTable(
                name: "ReadModelRebuildRun",
                schema: "Configuration",
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
                name: "SystemSettingChange",
                schema: "Configuration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID"),
                    Key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "设置键"),
                    OldValue = table.Column<string>(type: "jsonb", nullable: true, comment: "改之前的值；此前用的是默认值时为空"),
                    NewValue = table.Column<string>(type: "jsonb", nullable: true, comment: "改之后的值；恢复默认时为空"),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "原因，必填"),
                    TraceId = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true, comment: "链路追踪 ID，可以到日志与链路里查这次修改"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "创建人"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "修改时间"),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "修改人"),
                    ConcurrencyVersion = table.Column<long>(type: "bigint", nullable: false, comment: "并发版本号")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettingChange", x => x.Id);
                },
                comment: "系统设置的变更历史；Configuration 模块；只追加，运行账号没有修改与删除权限；谁改的、什么时候改的是创建人和创建时间");

            migrationBuilder.CreateTable(
                name: "SystemSettingValue",
                schema: "Configuration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID"),
                    Key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "设置键，以声明它的模块名开头，如 Sample.Submission"),
                    Value = table.Column<string>(type: "jsonb", nullable: false, comment: "值，JSON 对象"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "创建人"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "修改时间"),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "修改人"),
                    ConcurrencyVersion = table.Column<long>(type: "bigint", nullable: false, comment: "并发版本号")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettingValue", x => x.Id);
                },
                comment: "被修改过的系统设置值；Configuration 模块；一个设置一行，没有这一行就用代码里的默认值");

            migrationBuilder.CreateTable(
                name: "DictionaryItem",
                schema: "Configuration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID"),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "编码：同一字典类型内唯一，不区分大小写，创建后不能改", collation: "CaseInsensitive"),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "显示名"),
                    ParentCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "层级字典里父项的编码；顶层为空"),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, comment: "在同一父项下的次序"),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "状态"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "创建人"),
                    TypeId = table.Column<Guid>(type: "uuid", nullable: false, comment: "DictionaryType ID"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "修改时间"),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "修改人")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DictionaryItem", x => x.Id);
                    table.CheckConstraint("CK_DictionaryItem_Status", "\"Status\" IN ('Active', 'Disabled')");
                    table.ForeignKey(
                        name: "FK_DictionaryItem_DictionaryType_TypeId",
                        column: x => x.TypeId,
                        principalSchema: "Configuration",
                        principalTable: "DictionaryType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "字典项；Configuration 模块；业务数据里存的是它的编码；不删除只停用");

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundJob_ExpiresAt",
                schema: "Configuration",
                table: "BackgroundJob",
                column: "ExpiresAt",
                filter: "\"ExpiresAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundJob_LeaseUntil",
                schema: "Configuration",
                table: "BackgroundJob",
                column: "LeaseUntil",
                filter: "\"Status\" IN (0, 1, 4)");

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundJob_SubmittedBy_SubmittedAt",
                schema: "Configuration",
                table: "BackgroundJob",
                columns: new[] { "SubmittedBy", "SubmittedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_DictionaryItem_TypeId_Code",
                schema: "Configuration",
                table: "DictionaryItem",
                columns: new[] { "TypeId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DictionaryType_Code",
                schema: "Configuration",
                table: "DictionaryType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DictionaryType_CreatedAt",
                schema: "Configuration",
                table: "DictionaryType",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_IdempotencyRecord_ExpiresAt",
                schema: "Configuration",
                table: "IdempotencyRecord",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_IdempotencyRecord_UserId_RoleId_Operation_Key",
                schema: "Configuration",
                table: "IdempotencyRecord",
                columns: new[] { "UserId", "RoleId", "Operation", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessage_MessageId_Subscription",
                schema: "Configuration",
                table: "InboxMessage",
                columns: new[] { "MessageId", "Subscription" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessage_NextAttemptAt",
                schema: "Configuration",
                table: "InboxMessage",
                column: "NextAttemptAt",
                filter: "\"Status\" IN (0, 1)");

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessage_ReceivedAt",
                schema: "Configuration",
                table: "InboxMessage",
                column: "ReceivedAt",
                filter: "\"Status\" = 2");

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessage_SubjectIds",
                schema: "Configuration",
                table: "InboxMessage",
                column: "SubjectIds",
                filter: "\"SubjectIds\" IS NOT NULL AND \"Status\" IN (0, 1, 3)")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessage_Subscription",
                schema: "Configuration",
                table: "InboxMessage",
                column: "Subscription",
                filter: "\"Status\" IN (0, 1, 3)");

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessage_Subscription_OrderingKey_Seq",
                schema: "Configuration",
                table: "InboxMessage",
                columns: new[] { "Subscription", "OrderingKey", "Seq" },
                filter: "\"Status\" IN (0, 1, 3) AND \"OrderingKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_NextAttemptAt",
                schema: "Configuration",
                table: "OutboxMessage",
                column: "NextAttemptAt",
                filter: "\"Status\" IN (0, 1)");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_Status_OccurredAt",
                schema: "Configuration",
                table: "OutboxMessage",
                columns: new[] { "Status", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_SubjectIds",
                schema: "Configuration",
                table: "OutboxMessage",
                column: "SubjectIds",
                filter: "\"SubjectIds\" IS NOT NULL AND \"Status\" IN (0, 1, 3)")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_PreferenceValue_CreatedAt",
                schema: "Configuration",
                table: "PreferenceValue",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PreferenceValue_Key_Layer_RoleId_UserId",
                schema: "Configuration",
                table: "PreferenceValue",
                columns: new[] { "Key", "Layer", "RoleId", "UserId" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_PreferenceValue_Layer_RoleId_UserId",
                schema: "Configuration",
                table: "PreferenceValue",
                columns: new[] { "Layer", "RoleId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_ReadModelRebuildRun_Subscription",
                schema: "Configuration",
                table: "ReadModelRebuildRun",
                column: "Subscription",
                unique: true,
                filter: "\"Status\" IN (0, 1)");

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettingChange_CreatedAt",
                schema: "Configuration",
                table: "SystemSettingChange",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettingChange_Key",
                schema: "Configuration",
                table: "SystemSettingChange",
                column: "Key");

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettingValue_CreatedAt",
                schema: "Configuration",
                table: "SystemSettingValue",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettingValue_Key",
                schema: "Configuration",
                table: "SystemSettingValue",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BackgroundJob",
                schema: "Configuration");

            migrationBuilder.DropTable(
                name: "DictionaryItem",
                schema: "Configuration");

            migrationBuilder.DropTable(
                name: "IdempotencyRecord",
                schema: "Configuration");

            migrationBuilder.DropTable(
                name: "InboxMessage",
                schema: "Configuration");

            migrationBuilder.DropTable(
                name: "NumberCounter",
                schema: "Configuration");

            migrationBuilder.DropTable(
                name: "OutboxMessage",
                schema: "Configuration");

            migrationBuilder.DropTable(
                name: "PreferenceValue",
                schema: "Configuration");

            migrationBuilder.DropTable(
                name: "ReadModelRebuildRun",
                schema: "Configuration");

            migrationBuilder.DropTable(
                name: "SystemSettingChange",
                schema: "Configuration");

            migrationBuilder.DropTable(
                name: "SystemSettingValue",
                schema: "Configuration");

            migrationBuilder.DropTable(
                name: "DictionaryType",
                schema: "Configuration");
        }
    }
}
