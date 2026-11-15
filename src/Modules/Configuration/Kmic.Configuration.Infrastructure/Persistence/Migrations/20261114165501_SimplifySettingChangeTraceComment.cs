using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kmic.Configuration.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SimplifySettingChangeTraceComment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TraceId",
                schema: "Configuration",
                table: "SystemSettingChange",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true,
                comment: "链路追踪 ID",
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32,
                oldNullable: true,
                oldComment: "链路追踪 ID，可以到日志与链路里查这次修改");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TraceId",
                schema: "Configuration",
                table: "SystemSettingChange",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true,
                comment: "链路追踪 ID，可以到日志与链路里查这次修改",
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32,
                oldNullable: true,
                oldComment: "链路追踪 ID");
        }
    }
}
