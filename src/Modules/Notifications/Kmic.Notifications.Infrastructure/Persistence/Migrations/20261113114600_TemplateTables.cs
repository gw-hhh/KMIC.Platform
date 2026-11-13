using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kmic.Notifications.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TemplateTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotificationTemplate",
                schema: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID"),
                    Key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "模板键，以声明它的模块名开头，唯一"),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "显示名"),
                    OwnerModule = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "声明它的模块"),
                    Channels = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "发往的渠道，渠道编码用逗号隔开"),
                    SenderProfile = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "发件账号名，只对邮件渠道有意义"),
                    CarriesSecret = table.Column<bool>(type: "boolean", nullable: false, comment: "内容含一次性短时凭据（如验证码）"),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "状态"),
                    CodeHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "代码里声明内容的哈希，同步时用来判断有没有变化"),
                    ActiveVersions = table.Column<string>(type: "jsonb", nullable: false, comment: "各渠道当前生效的版本号，JSON：渠道编码到版本号；没有的渠道用代码里的默认内容"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "创建人"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "修改时间"),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "修改人"),
                    ConcurrencyVersion = table.Column<long>(type: "bigint", nullable: false, comment: "并发版本号")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTemplate", x => x.Id);
                    table.CheckConstraint("CK_NotificationTemplate_Status", "\"Status\" IN ('Active', 'Retired')");
                },
                comment: "通知模板；Notifications 模块；一行是代码里声明的一种通知，启动时与代码同步；各渠道发布过的内容版本在子表");

            migrationBuilder.CreateTable(
                name: "TemplateVersion",
                schema: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID"),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: false, comment: "所属模板"),
                    Version = table.Column<int>(type: "integer", nullable: false, comment: "版本号，同一个模板内递增"),
                    Channel = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "渠道"),
                    Subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "主题；站内消息里是标题"),
                    Body = table.Column<string>(type: "character varying(20000)", maxLength: 20000, nullable: false, comment: "正文"),
                    PublishedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "发布人"),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "发布时间"),
                    Remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "备注"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "创建人"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "修改时间"),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "修改人")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateVersion", x => x.Id);
                    table.CheckConstraint("CK_TemplateVersion_Channel", "\"Channel\" IN ('Email', 'Sms', 'InApp')");
                    table.ForeignKey(
                        name: "FK_TemplateVersion_NotificationTemplate_TemplateId",
                        column: x => x.TemplateId,
                        principalSchema: "Notifications",
                        principalTable: "NotificationTemplate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "通知模板的发布版本；Notifications 模块；一行是模板在某个渠道发布过的一版内容，只增不改");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplate_CreatedAt",
                schema: "Notifications",
                table: "NotificationTemplate",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplate_Key",
                schema: "Notifications",
                table: "NotificationTemplate",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TemplateVersion_TemplateId_Version",
                schema: "Notifications",
                table: "TemplateVersion",
                columns: new[] { "TemplateId", "Version" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TemplateVersion",
                schema: "Notifications");

            migrationBuilder.DropTable(
                name: "NotificationTemplate",
                schema: "Notifications");
        }
    }
}
