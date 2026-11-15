using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kmic.Notifications.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ClarifyModelComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Address",
                schema: "Notifications",
                table: "CapturedMessage",
                type: "character varying(320)",
                maxLength: 320,
                nullable: false,
                comment: "收件地址（未脱敏）",
                oldClrType: typeof(string),
                oldType: "character varying(320)",
                oldMaxLength: 320,
                oldComment: "本该发往的完整地址");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                schema: "Notifications",
                table: "CapturedMessage",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                comment: "邮件收件方式",
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldComment: "邮件里的角色");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                schema: "Notifications",
                table: "Delivery",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                comment: "邮件收件方式",
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldComment: "邮件里的角色");

            migrationBuilder.AlterColumn<string>(
                name: "ActiveVersions",
                schema: "Notifications",
                table: "NotificationTemplate",
                type: "jsonb",
                nullable: false,
                comment: "各渠道选用的发布版本号（JSON：渠道编码到版本号）",
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldComment: "各渠道当前生效的版本号，JSON：渠道编码到版本号；没有的渠道用代码里的默认内容");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ActiveVersions",
                schema: "Notifications",
                table: "NotificationTemplate",
                type: "jsonb",
                nullable: false,
                comment: "各渠道当前生效的版本号，JSON：渠道编码到版本号；没有的渠道用代码里的默认内容",
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldComment: "各渠道选用的发布版本号（JSON：渠道编码到版本号）");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                schema: "Notifications",
                table: "Delivery",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                comment: "邮件里的角色",
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldComment: "邮件收件方式");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                schema: "Notifications",
                table: "CapturedMessage",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                comment: "邮件里的角色",
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldComment: "邮件收件方式");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                schema: "Notifications",
                table: "CapturedMessage",
                type: "character varying(320)",
                maxLength: 320,
                nullable: false,
                comment: "本该发往的完整地址",
                oldClrType: typeof(string),
                oldType: "character varying(320)",
                oldMaxLength: 320,
                oldComment: "收件地址（未脱敏）");
        }
    }
}
