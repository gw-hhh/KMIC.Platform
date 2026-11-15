using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kmic.Operations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ClarifyJobSettingNullComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "Enabled",
                schema: "Operations",
                table: "ScheduledJobSetting",
                type: "boolean",
                nullable: true,
                comment: "是否启用；为 null 时使用默认值（启用）",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true,
                oldComment: "是否启用；空表示跟着代码里的默认值（启用）");

            migrationBuilder.AlterColumn<string>(
                name: "Cron",
                schema: "Operations",
                table: "ScheduledJobSetting",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true,
                comment: "Cron 表达式；为 null 时使用代码中的默认表达式",
                oldClrType: typeof(string),
                oldType: "character varying(120)",
                oldMaxLength: 120,
                oldNullable: true,
                oldComment: "执行时间的 Cron 表达式；空表示跟着代码里的默认值");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "Enabled",
                schema: "Operations",
                table: "ScheduledJobSetting",
                type: "boolean",
                nullable: true,
                comment: "是否启用；空表示跟着代码里的默认值（启用）",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true,
                oldComment: "是否启用；为 null 时使用默认值（启用）");

            migrationBuilder.AlterColumn<string>(
                name: "Cron",
                schema: "Operations",
                table: "ScheduledJobSetting",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true,
                comment: "执行时间的 Cron 表达式；空表示跟着代码里的默认值",
                oldClrType: typeof(string),
                oldType: "character varying(120)",
                oldMaxLength: 120,
                oldNullable: true,
                oldComment: "Cron 表达式；为 null 时使用代码中的默认表达式");
        }
    }
}
