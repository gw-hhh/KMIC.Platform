using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kmic.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ClarifyModelComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsProtected",
                schema: "Identity",
                table: "Role",
                type: "boolean",
                nullable: false,
                comment: "是否受保护",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldComment: "是否受保护：只有持有受保护角色的人才能管理它，也不能借管理它的持有人绕开");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsProtected",
                schema: "Identity",
                table: "Role",
                type: "boolean",
                nullable: false,
                comment: "是否受保护：只有持有受保护角色的人才能管理它，也不能借管理它的持有人绕开",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldComment: "是否受保护");
        }
    }
}
