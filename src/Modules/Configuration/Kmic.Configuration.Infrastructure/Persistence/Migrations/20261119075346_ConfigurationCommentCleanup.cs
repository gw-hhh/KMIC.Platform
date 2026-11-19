using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kmic.Configuration.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConfigurationCommentCleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "OldValue",
                schema: "Configuration",
                table: "SystemSettingChange",
                type: "jsonb",
                nullable: true,
                comment: "修改前保存的值，原来没有保存记录时为空",
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true,
                oldComment: "修改前保存的值");

            migrationBuilder.AlterColumn<string>(
                name: "NewValue",
                schema: "Configuration",
                table: "SystemSettingChange",
                type: "jsonb",
                nullable: true,
                comment: "修改后保存的值，恢复默认或清除设置时为空",
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true,
                oldComment: "修改后保存的值");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "OldValue",
                schema: "Configuration",
                table: "SystemSettingChange",
                type: "jsonb",
                nullable: true,
                comment: "修改前保存的值",
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true,
                oldComment: "修改前保存的值，原来没有保存记录时为空");

            migrationBuilder.AlterColumn<string>(
                name: "NewValue",
                schema: "Configuration",
                table: "SystemSettingChange",
                type: "jsonb",
                nullable: true,
                comment: "修改后保存的值",
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true,
                oldComment: "修改后保存的值，恢复默认或清除设置时为空");
        }
    }
}
