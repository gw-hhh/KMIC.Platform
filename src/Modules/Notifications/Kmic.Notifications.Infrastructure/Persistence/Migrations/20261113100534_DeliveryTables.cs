using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kmic.Notifications.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DeliveryTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CapturedMessage",
                schema: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID"),
                    DeliveryId = table.Column<Guid>(type: "uuid", nullable: false, comment: "对应的投递"),
                    Channel = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "渠道"),
                    Address = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false, comment: "本该发往的完整地址"),
                    Role = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "邮件里的角色"),
                    Subject = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "主题"),
                    Body = table.Column<string>(type: "text", nullable: false, comment: "正文"),
                    CapturedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "捕获的时间"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "创建人"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "修改时间"),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "修改人"),
                    ConcurrencyVersion = table.Column<long>(type: "bigint", nullable: false, comment: "并发版本号")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CapturedMessage", x => x.Id);
                    table.CheckConstraint("CK_CapturedMessage_Channel", "\"Channel\" IN ('Email', 'Sms')");
                    table.CheckConstraint("CK_CapturedMessage_Role", "\"Role\" IN ('To', 'Cc', 'Bcc')");
                },
                comment: "非生产环境捕获的邮件与短信；Notifications 模块；本该发出去的内容留在这里供查看，只在非生产环境有数据，超过保留期由清理任务删除");

            migrationBuilder.CreateTable(
                name: "Delivery",
                schema: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID"),
                    NotificationId = table.Column<Guid>(type: "uuid", nullable: false, comment: "产生它的发送请求"),
                    Channel = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "渠道"),
                    RecipientUserId = table.Column<Guid>(type: "uuid", nullable: true, comment: "收件人，系统里的用户；直接给了地址的收件人为空"),
                    RecipientKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "去重用的收件人键：用户是 u:用户 ID，直接给的地址是 a:地址小写的 SHA-256"),
                    AddressMasked = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "脱敏后的地址，管理页和日志里只显示它"),
                    Address = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true, comment: "直接给的地址；收件人是用户时为空，发送时再向 Identity 取最新的联系方式"),
                    Role = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "邮件里的角色"),
                    Subject = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "渲染好的主题"),
                    Body = table.Column<string>(type: "text", nullable: true, comment: "渲染好的正文；含密模板投递完成后清空"),
                    TemplateKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "模板键"),
                    DedupKey = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false, comment: "业务给的去重键：同一个模板、收件人、去重键、渠道只有一条投递"),
                    TemplateVersion = table.Column<int>(type: "integer", nullable: false, comment: "实际用的模板版本号，0 表示代码里的默认内容"),
                    CodeHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "渲染时代码里默认内容的哈希，事后能知道发出的是哪一版默认内容"),
                    Priority = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "优先级"),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "状态"),
                    SkipReason = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true, comment: "被跳过的原因，只有已跳过的才有"),
                    Attempts = table.Column<int>(type: "integer", nullable: false, comment: "已经尝试发送几次"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "创建人"),
                    LastError = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true, comment: "最近一次失败的原因"),
                    LeaseToken = table.Column<Guid>(type: "uuid", nullable: true, comment: "投递工作器的领取令牌"),
                    LeaseUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "领取的到期时间"),
                    NextAttemptAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "下次可以领取的时间"),
                    ProviderMessageId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true, comment: "渠道返回的消息标识"),
                    SentAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "发送成功的时间"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "修改时间"),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "修改人"),
                    ConcurrencyVersion = table.Column<long>(type: "bigint", nullable: false, comment: "并发版本号")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Delivery", x => x.Id);
                    table.CheckConstraint("CK_Delivery_Channel", "\"Channel\" IN ('Email', 'Sms')");
                    table.CheckConstraint("CK_Delivery_Priority", "\"Priority\" IN ('Normal', 'High')");
                    table.CheckConstraint("CK_Delivery_Role", "\"Role\" IN ('To', 'Cc', 'Bcc')");
                    table.CheckConstraint("CK_Delivery_SkipReason", "\"SkipReason\" IN ('NoAddress', 'UserDisabled', 'UnknownUser')");
                    table.CheckConstraint("CK_Delivery_Status", "\"Status\" IN ('Pending', 'Sending', 'Sent', 'Failed', 'Skipped', 'Captured')");
                },
                comment: "通知的投递；Notifications 模块；一行是把一条通知用一个渠道发给一个收件人，渲染好的内容随行保存；状态靠带条件的语句更新，不经聚合");

            migrationBuilder.CreateTable(
                name: "Notification",
                schema: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID"),
                    TemplateKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "模板键"),
                    TemplateVersions = table.Column<string>(type: "jsonb", nullable: false, comment: "本次各渠道实际用的版本号，JSON：渠道编码到版本号，0 表示代码里的默认内容"),
                    DedupKey = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false, comment: "业务给的去重键"),
                    SourceMessageId = table.Column<Guid>(type: "uuid", nullable: false, comment: "来源消息的 ID，唯一"),
                    SourceModule = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "发出请求的模块"),
                    InitiatorUserId = table.Column<Guid>(type: "uuid", nullable: true, comment: "发起这件事的用户，系统发起的为空"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "创建人"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "修改时间"),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "修改人"),
                    ConcurrencyVersion = table.Column<long>(type: "bigint", nullable: false, comment: "并发版本号")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.Id);
                },
                comment: "通知发送请求的记录；Notifications 模块；一行是一次发送请求，处理时插入，之后不再修改；来源消息 ID 唯一，保证同一条消息只处理一次");

            migrationBuilder.CreateIndex(
                name: "IX_CapturedMessage_CapturedAt",
                schema: "Notifications",
                table: "CapturedMessage",
                column: "CapturedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CapturedMessage_CreatedAt",
                schema: "Notifications",
                table: "CapturedMessage",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CapturedMessage_DeliveryId",
                schema: "Notifications",
                table: "CapturedMessage",
                column: "DeliveryId");

            migrationBuilder.CreateIndex(
                name: "IX_Delivery_CreatedAt",
                schema: "Notifications",
                table: "Delivery",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Delivery_NotificationId",
                schema: "Notifications",
                table: "Delivery",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_Delivery_RecipientUserId",
                schema: "Notifications",
                table: "Delivery",
                column: "RecipientUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Delivery_TemplateKey_RecipientKey_DedupKey_Channel",
                schema: "Notifications",
                table: "Delivery",
                columns: new[] { "TemplateKey", "RecipientKey", "DedupKey", "Channel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notification_CreatedAt",
                schema: "Notifications",
                table: "Notification",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_SourceMessageId",
                schema: "Notifications",
                table: "Notification",
                column: "SourceMessageId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CapturedMessage",
                schema: "Notifications");

            migrationBuilder.DropTable(
                name: "Delivery",
                schema: "Notifications");

            migrationBuilder.DropTable(
                name: "Notification",
                schema: "Notifications");
        }
    }
}
