using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kmic.Notifications.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateChineseComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "CapturedMessage",
                schema: "Notifications",
                comment: "捕获内容（Notifications）：一条记录对应一次邮件或短信投递；开启捕获时保存内容而不实际发送，按保留期清理",
                oldComment: "非生产环境捕获的邮件与短信；Notifications 模块；本该发出去的内容留在这里供查看，只在非生产环境有数据，超过保留期由清理任务删除");

            migrationBuilder.AlterColumn<string>(
                name: "SmsParameters",
                schema: "Notifications",
                table: "Delivery",
                type: "jsonb",
                nullable: true,
                comment: "短信模板参数（JSON）",
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true,
                oldComment: "短信模板的参数，JSON；只有短信有。短信服务只收模板编码和参数，实际发送用它，含密模板投递完成后清空");

            migrationBuilder.AlterColumn<string>(
                name: "CodeHash",
                schema: "Notifications",
                table: "Delivery",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                comment: "模板声明及默认内容的哈希",
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldComment: "渲染时代码里默认内容的哈希，事后能知道发出的是哪一版默认内容");

            migrationBuilder.AlterColumn<string>(
                name: "Body",
                schema: "Notifications",
                table: "Delivery",
                type: "text",
                nullable: true,
                comment: "渲染后的正文",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "渲染好的正文；含密模板投递完成后清空");

            migrationBuilder.AlterColumn<int>(
                name: "Attempts",
                schema: "Notifications",
                table: "Delivery",
                type: "integer",
                nullable: false,
                comment: "领取次数，人工重试时清零",
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "已经尝试发送几次");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "CapturedMessage",
                schema: "Notifications",
                comment: "非生产环境捕获的邮件与短信；Notifications 模块；本该发出去的内容留在这里供查看，只在非生产环境有数据，超过保留期由清理任务删除",
                oldComment: "捕获内容（Notifications）：一条记录对应一次邮件或短信投递；开启捕获时保存内容而不实际发送，按保留期清理");

            migrationBuilder.AlterColumn<string>(
                name: "SmsParameters",
                schema: "Notifications",
                table: "Delivery",
                type: "jsonb",
                nullable: true,
                comment: "短信模板的参数，JSON；只有短信有。短信服务只收模板编码和参数，实际发送用它，含密模板投递完成后清空",
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true,
                oldComment: "短信模板参数（JSON）");

            migrationBuilder.AlterColumn<string>(
                name: "CodeHash",
                schema: "Notifications",
                table: "Delivery",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                comment: "渲染时代码里默认内容的哈希，事后能知道发出的是哪一版默认内容",
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldComment: "模板声明及默认内容的哈希");

            migrationBuilder.AlterColumn<string>(
                name: "Body",
                schema: "Notifications",
                table: "Delivery",
                type: "text",
                nullable: true,
                comment: "渲染好的正文；含密模板投递完成后清空",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "渲染后的正文");

            migrationBuilder.AlterColumn<int>(
                name: "Attempts",
                schema: "Notifications",
                table: "Delivery",
                type: "integer",
                nullable: false,
                comment: "已经尝试发送几次",
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "领取次数，人工重试时清零");
        }
    }
}
