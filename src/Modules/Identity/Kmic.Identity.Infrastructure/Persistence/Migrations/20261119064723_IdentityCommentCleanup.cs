using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kmic.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IdentityCommentCleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Mobile",
                schema: "Identity",
                table: "User",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                comment: "手机号，国际格式，填写时唯一",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true,
                oldComment: "手机号");

            migrationBuilder.AlterColumn<string>(
                name: "Account",
                schema: "Identity",
                table: "User",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                comment: "登录名，唯一，不区分大小写",
                collation: "CaseInsensitive",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldComment: "登录名",
                oldCollation: "CaseInsensitive");

            migrationBuilder.AlterColumn<string>(
                name: "CatalogFingerprint",
                schema: "Identity",
                table: "Role",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                comment: "全权限角色最近一次同步的权限点目录指纹",
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true,
                oldComment: "全权限角色最近一次同步的权限点目录指纹；目录变化时同步任务更新它，让授权版本跟着递增");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Mobile",
                schema: "Identity",
                table: "User",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                comment: "手机号",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true,
                oldComment: "手机号，国际格式，填写时唯一");

            migrationBuilder.AlterColumn<string>(
                name: "Account",
                schema: "Identity",
                table: "User",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                comment: "登录名",
                collation: "CaseInsensitive",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldComment: "登录名，唯一，不区分大小写",
                oldCollation: "CaseInsensitive");

            migrationBuilder.AlterColumn<string>(
                name: "CatalogFingerprint",
                schema: "Identity",
                table: "Role",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                comment: "全权限角色最近一次同步的权限点目录指纹；目录变化时同步任务更新它，让授权版本跟着递增",
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true,
                oldComment: "全权限角色最近一次同步的权限点目录指纹");
        }
    }
}
