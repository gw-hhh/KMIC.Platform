using System;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Infrastructure.SecurityEvents;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Kmic.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitializeModuleSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InitializeModuleSchema("Identity");

            migrationBuilder.EnsureSchema(
                name: "Identity");

            migrationBuilder.CreateTable(
                name: "BackgroundJob",
                schema: "Identity",
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
                schema: "Identity",
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
                schema: "Identity",
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
                schema: "Identity",
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
                name: "OrgTree",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "创建人"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "修改时间"),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "修改人"),
                    ConcurrencyVersion = table.Column<long>(type: "bigint", nullable: false, comment: "并发版本号")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrgTree", x => x.Id);
                },
                comment: "组织树；Identity 模块；只有一行，整棵树一个聚合；并发版本号即组织树版本号");

            migrationBuilder.CreateTable(
                name: "OutboxMessage",
                schema: "Identity",
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
                schema: "Identity",
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
                name: "Role",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "名称，唯一且不区分大小写", collation: "CaseInsensitive"),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "说明"),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "状态"),
                    IsProtected = table.Column<bool>(type: "boolean", nullable: false, comment: "是否受保护：只有持有受保护角色的人才能管理它，也不能借管理它的持有人绕开"),
                    GrantsAllPermissions = table.Column<bool>(type: "boolean", nullable: false, comment: "是否拥有全部权限点；为真时不存权限点行，新增的权限点自动包含在内"),
                    DefaultScopeLevel = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "默认的数据范围等级，没有单独设置的范围对象都按它"),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, comment: "登录时角色下拉里的次序"),
                    CatalogFingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true, comment: "全权限角色最近一次同步的权限点目录指纹；目录变化时同步任务更新它，让授权版本跟着递增"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "创建人"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "修改时间"),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "修改人"),
                    ConcurrencyVersion = table.Column<long>(type: "bigint", nullable: false, comment: "并发版本号")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.Id);
                    table.CheckConstraint("CK_Role_DefaultScopeLevel", "\"DefaultScopeLevel\" IN ('Self', 'OrgUnit', 'OrgUnitAndBelow', 'All')");
                    table.CheckConstraint("CK_Role_Status", "\"Status\" IN ('Active', 'Disabled')");
                },
                comment: "角色；Identity 模块；一行代表一个角色（一组权限点加数据范围）；不删除只停用；并发版本号即授权版本号");

            migrationBuilder.CreateTable(
                name: "RoleStateHistory",
                schema: "Identity",
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
                    table.PrimaryKey("PK_RoleStateHistory", x => x.Id);
                },
                comment: "Role 的状态流转记录，只追加");

            migrationBuilder.CreateTable(
                name: "Session",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID"),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false, comment: "用户；模拟会话里是被模拟的人"),
                    CurrentRoleId = table.Column<Guid>(type: "uuid", nullable: false, comment: "当前角色"),
                    ClientType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "客户端类型"),
                    DeviceName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true, comment: "设备名称，App 上报或由 User-Agent 解析"),
                    LoginIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true, comment: "登录时的客户端 IP"),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "状态"),
                    RevokedReason = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true, comment: "作废原因"),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "作废时间"),
                    LastRefreshedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "最近一次刷新的时间，只在刷新时更新，不在每个请求更新"),
                    RefreshExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "滑动过期时间"),
                    AbsoluteExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "绝对上限：无论怎样刷新，会话最晚在这个时间失效"),
                    ImpersonatorUserId = table.Column<Guid>(type: "uuid", nullable: true, comment: "模拟身份时的实际操作人；普通会话为空"),
                    ImpersonationAllowsWrite = table.Column<bool>(type: "boolean", nullable: false, comment: "模拟会话是否允许修改数据，创建时确定，会话期间不变"),
                    ImpersonationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "开始模拟的原因"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "创建人"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "修改时间"),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "修改人"),
                    ConcurrencyVersion = table.Column<long>(type: "bigint", nullable: false, comment: "并发版本号")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Session", x => x.Id);
                    table.CheckConstraint("CK_Session_ClientType", "\"ClientType\" IN ('Web', 'App')");
                    table.CheckConstraint("CK_Session_RevokedReason", "\"RevokedReason\" IN ('Logout', 'PasswordChanged', 'PasswordReset', 'UserDisabled', 'ForcedByAdmin', 'RefreshTokenReused', 'ImpersonationEnded')");
                    table.CheckConstraint("CK_Session_Status", "\"Status\" IN ('Active', 'Revoked')");
                },
                comment: "会话；Identity 模块；一行代表一次登录（含模拟身份）；作废或过期 30 天后由清理任务删除");

            migrationBuilder.CreateTable(
                name: "User",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID"),
                    Account = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "登录名，唯一且不区分大小写；迁移来的用户沿用旧系统的手机号", collation: "CaseInsensitive"),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "姓名，别的模块显示人名时经引用查询取"),
                    EnglishName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "英文名，合同、报告上要印"),
                    EmployeeNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "工号，填写时唯一", collation: "CaseInsensitive"),
                    Mobile = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, comment: "手机号，规范化为国际格式，填写时唯一；短信登录需要它"),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true, comment: "邮箱"),
                    OrgUnitId = table.Column<Guid>(type: "uuid", nullable: true, comment: "所属组织单元，数据范围的“本组织”从它算起；为空时只剩“本人”的数据"),
                    Kind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "种类"),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "状态"),
                    PasswordHash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true, comment: "密码哈希；为空表示还没有设置过密码（迁移来的员工靠短信登录）"),
                    PasswordChangeRequired = table.Column<bool>(type: "boolean", nullable: false, comment: "登录后是否必须先修改密码"),
                    PasswordChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "密码最近一次变化的时间"),
                    LastRoleId = table.Column<Guid>(type: "uuid", nullable: true, comment: "上次使用的角色，下次登录时优先选它"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "创建人"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "修改时间"),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "修改人"),
                    ConcurrencyVersion = table.Column<long>(type: "bigint", nullable: false, comment: "并发版本号")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.CheckConstraint("CK_User_Kind", "\"Kind\" IN ('Normal', 'System')");
                    table.CheckConstraint("CK_User_Status", "\"Status\" IN ('Active', 'Disabled')");
                },
                comment: "用户；Identity 模块；一行代表一个能登录系统的人或代表系统的固定账号；不删除只禁用");

            migrationBuilder.CreateTable(
                name: "UserStateHistory",
                schema: "Identity",
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
                    table.PrimaryKey("PK_UserStateHistory", x => x.Id);
                },
                comment: "User 的状态流转记录，只追加");

            migrationBuilder.CreateTable(
                name: "OrgUnit",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID"),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true, comment: "上级；根单元为空"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "名称，同一上级下不重复（不区分大小写）", collation: "CaseInsensitive"),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "编码，可空；业务上需要稳定标识一个单元时使用"),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "状态"),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, comment: "同一上级下的次序"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "创建人"),
                    TreeId = table.Column<Guid>(type: "uuid", nullable: false, comment: "OrgTree ID"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "修改时间"),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "修改人")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrgUnit", x => x.Id);
                    table.CheckConstraint("CK_OrgUnit_Status", "\"Status\" IN ('Active', 'Disabled')");
                    table.ForeignKey(
                        name: "FK_OrgUnit_OrgTree_TreeId",
                        column: x => x.TreeId,
                        principalSchema: "Identity",
                        principalTable: "OrgTree",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "组织单元；Identity 模块；一行代表组织树上的一个节点，随组织树一起保存");

            migrationBuilder.CreateTable(
                name: "RoleDataScope",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID"),
                    ScopeObject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "范围对象编码，如 Sample.Specimen"),
                    Level = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "等级"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "创建人"),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false, comment: "Role ID"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "修改时间"),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "修改人")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleDataScope", x => x.Id);
                    table.CheckConstraint("CK_RoleDataScope_Level", "\"Level\" IN ('Self', 'OrgUnit', 'OrgUnitAndBelow', 'All')");
                    table.ForeignKey(
                        name: "FK_RoleDataScope_Role_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "Identity",
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "角色的数据范围覆盖；Identity 模块；一行代表角色对一个范围对象单独设置的等级，随角色一起保存");

            migrationBuilder.CreateTable(
                name: "RolePermissionEntry",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID"),
                    PermissionCode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "权限点编码，如 Sample.Specimen.View"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "创建人"),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false, comment: "Role ID"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "修改时间"),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "修改人")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissionEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermissionEntry_Role_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "Identity",
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "角色的权限点；Identity 模块；一行代表角色拥有一个权限点，随角色一起保存");

            migrationBuilder.CreateTable(
                name: "RefreshToken",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID"),
                    TokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "令牌的 SHA-256 哈希，小写十六进制"),
                    IssuedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "签发时间"),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "过期时间，不会超过会话的绝对上限"),
                    UsedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "被使用（换成新令牌）的时间；为空表示它是会话当前的令牌"),
                    ReplacedById = table.Column<Guid>(type: "uuid", nullable: true, comment: "接替它的新令牌"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "创建人"),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false, comment: "Session ID"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "修改时间"),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "修改人")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshToken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshToken_Session_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "Identity",
                        principalTable: "Session",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "刷新令牌；Identity 模块；一行代表签发过的一个刷新令牌（只存哈希），随会话一起保存");

            migrationBuilder.CreateTable(
                name: "UserRoleGrant",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID"),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false, comment: "角色"),
                    GrantedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "授予人"),
                    GrantedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "授予时间"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "创建人"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "修改时间"),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "修改人"),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false, comment: "User ID")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoleGrant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoleGrant_User_UserId",
                        column: x => x.UserId,
                        principalSchema: "Identity",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "用户的角色授予；Identity 模块；一行代表用户拥有一个角色，随用户一起保存");

            migrationBuilder.CreateTable(
                name: "GrantAnchor",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID"),
                    OrgUnitId = table.Column<Guid>(type: "uuid", nullable: false, comment: "起点组织单元"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "创建人"),
                    GrantId = table.Column<Guid>(type: "uuid", nullable: false, comment: "UserRoleGrant ID"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "修改时间"),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "修改人")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrantAnchor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrantAnchor_UserRoleGrant_GrantId",
                        column: x => x.GrantId,
                        principalSchema: "Identity",
                        principalTable: "UserRoleGrant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "授予的锚点；Identity 模块；一行代表授予上指定的一个起点组织单元，随授予一起保存");

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundJob_ExpiresAt",
                schema: "Identity",
                table: "BackgroundJob",
                column: "ExpiresAt",
                filter: "\"ExpiresAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundJob_LeaseUntil",
                schema: "Identity",
                table: "BackgroundJob",
                column: "LeaseUntil",
                filter: "\"Status\" IN (0, 1, 4)");

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundJob_SubmittedBy_SubmittedAt",
                schema: "Identity",
                table: "BackgroundJob",
                columns: new[] { "SubmittedBy", "SubmittedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_GrantAnchor_GrantId",
                schema: "Identity",
                table: "GrantAnchor",
                column: "GrantId");

            migrationBuilder.CreateIndex(
                name: "IX_GrantAnchor_OrgUnitId",
                schema: "Identity",
                table: "GrantAnchor",
                column: "OrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_IdempotencyRecord_ExpiresAt",
                schema: "Identity",
                table: "IdempotencyRecord",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_IdempotencyRecord_UserId_RoleId_Operation_Key",
                schema: "Identity",
                table: "IdempotencyRecord",
                columns: new[] { "UserId", "RoleId", "Operation", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessage_MessageId_Subscription",
                schema: "Identity",
                table: "InboxMessage",
                columns: new[] { "MessageId", "Subscription" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessage_NextAttemptAt",
                schema: "Identity",
                table: "InboxMessage",
                column: "NextAttemptAt",
                filter: "\"Status\" IN (0, 1)");

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessage_ReceivedAt",
                schema: "Identity",
                table: "InboxMessage",
                column: "ReceivedAt",
                filter: "\"Status\" = 2");

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessage_SubjectIds",
                schema: "Identity",
                table: "InboxMessage",
                column: "SubjectIds",
                filter: "\"SubjectIds\" IS NOT NULL AND \"Status\" IN (0, 1, 3)")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessage_Subscription",
                schema: "Identity",
                table: "InboxMessage",
                column: "Subscription",
                filter: "\"Status\" IN (0, 1, 3)");

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessage_Subscription_OrderingKey_Seq",
                schema: "Identity",
                table: "InboxMessage",
                columns: new[] { "Subscription", "OrderingKey", "Seq" },
                filter: "\"Status\" IN (0, 1, 3) AND \"OrderingKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OrgTree_CreatedAt",
                schema: "Identity",
                table: "OrgTree",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OrgUnit_ParentId_Name",
                schema: "Identity",
                table: "OrgUnit",
                columns: new[] { "ParentId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrgUnit_TreeId",
                schema: "Identity",
                table: "OrgUnit",
                column: "TreeId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_NextAttemptAt",
                schema: "Identity",
                table: "OutboxMessage",
                column: "NextAttemptAt",
                filter: "\"Status\" IN (0, 1)");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_Status_OccurredAt",
                schema: "Identity",
                table: "OutboxMessage",
                columns: new[] { "Status", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_SubjectIds",
                schema: "Identity",
                table: "OutboxMessage",
                column: "SubjectIds",
                filter: "\"SubjectIds\" IS NOT NULL AND \"Status\" IN (0, 1, 3)")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_ReadModelRebuildRun_Subscription",
                schema: "Identity",
                table: "ReadModelRebuildRun",
                column: "Subscription",
                unique: true,
                filter: "\"Status\" IN (0, 1)");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_SessionId",
                schema: "Identity",
                table: "RefreshToken",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_TokenHash",
                schema: "Identity",
                table: "RefreshToken",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Role_CreatedAt",
                schema: "Identity",
                table: "Role",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Role_Name",
                schema: "Identity",
                table: "Role",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleDataScope_RoleId",
                schema: "Identity",
                table: "RoleDataScope",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissionEntry_PermissionCode",
                schema: "Identity",
                table: "RolePermissionEntry",
                column: "PermissionCode");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissionEntry_RoleId",
                schema: "Identity",
                table: "RolePermissionEntry",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleStateHistory_AggregateId_OccurredAt",
                schema: "Identity",
                table: "RoleStateHistory",
                columns: new[] { "AggregateId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Session_CreatedAt",
                schema: "Identity",
                table: "Session",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Session_ImpersonatorUserId",
                schema: "Identity",
                table: "Session",
                column: "ImpersonatorUserId",
                filter: "\"ImpersonatorUserId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Session_UserId_Status",
                schema: "Identity",
                table: "Session",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_User_Account",
                schema: "Identity",
                table: "User",
                column: "Account",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_CreatedAt",
                schema: "Identity",
                table: "User",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_User_EmployeeNo",
                schema: "Identity",
                table: "User",
                column: "EmployeeNo",
                unique: true,
                filter: "\"EmployeeNo\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_User_Mobile",
                schema: "Identity",
                table: "User",
                column: "Mobile",
                unique: true,
                filter: "\"Mobile\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_User_OrgUnitId",
                schema: "Identity",
                table: "User",
                column: "OrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleGrant_RoleId",
                schema: "Identity",
                table: "UserRoleGrant",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleGrant_UserId_RoleId",
                schema: "Identity",
                table: "UserRoleGrant",
                columns: new[] { "UserId", "RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserStateHistory_AggregateId_OccurredAt",
                schema: "Identity",
                table: "UserStateHistory",
                columns: new[] { "AggregateId", "OccurredAt" });

            // 安全事件表属于本模块，建表语句由框架提供（只追加：运行账号改不了、删不了）
            migrationBuilder.Sql(SecurityEventTable.CreateSql);

            // 代表“系统”的固定账号：定时任务、事件处理这类没有登录用户的操作，审计列记为它。不能登录、不能授予、不能修改
            migrationBuilder.Sql("""
                INSERT INTO "Identity"."User" ("Id", "Account", "DisplayName", "Kind", "Status", "PasswordChangeRequired", "CreatedAt", "CreatedBy", "ConcurrencyVersion")
                VALUES ('00000000-0000-0000-0000-000000000001', 'system', '系统', 'System', 'Active', false, now(), '00000000-0000-0000-0000-000000000001', 0);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP TABLE IF EXISTS "Identity"."SecurityEvent";""");

            migrationBuilder.DropTable(
                name: "BackgroundJob",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "GrantAnchor",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "IdempotencyRecord",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "InboxMessage",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "NumberCounter",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "OrgUnit",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "OutboxMessage",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "ReadModelRebuildRun",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "RefreshToken",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "RoleDataScope",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "RolePermissionEntry",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "RoleStateHistory",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "UserStateHistory",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "UserRoleGrant",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "OrgTree",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "Session",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "Role",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "User",
                schema: "Identity");
        }
    }
}
