using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kmic.Files.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FilesCommentCleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UploadedAt",
                schema: "Files",
                table: "StoredFile",
                type: "timestamp with time zone",
                nullable: false,
                comment: "文件登记时间：上传文件是开始上传时，生成文件是处理登记消息时",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldComment: "文件登记时间");

            migrationBuilder.AlterColumn<string>(
                name: "Profile",
                schema: "Files",
                table: "StoredFile",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                comment: "上传配置名称，生成文件为 Generated:来源",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldComment: "上传配置名称");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ExpiresAt",
                schema: "Files",
                table: "StoredFile",
                type: "timestamp with time zone",
                nullable: true,
                comment: "清理到期时间，到期后由后台任务清理，不保证立即删除",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "清理到期时间");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UploadedAt",
                schema: "Files",
                table: "StoredFile",
                type: "timestamp with time zone",
                nullable: false,
                comment: "文件登记时间",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldComment: "文件登记时间：上传文件是开始上传时，生成文件是处理登记消息时");

            migrationBuilder.AlterColumn<string>(
                name: "Profile",
                schema: "Files",
                table: "StoredFile",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                comment: "上传配置名称",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldComment: "上传配置名称，生成文件为 Generated:来源");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ExpiresAt",
                schema: "Files",
                table: "StoredFile",
                type: "timestamp with time zone",
                nullable: true,
                comment: "清理到期时间",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "清理到期时间，到期后由后台任务清理，不保证立即删除");
        }
    }
}
