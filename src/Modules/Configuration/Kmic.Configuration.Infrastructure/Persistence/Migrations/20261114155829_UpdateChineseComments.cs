using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kmic.Configuration.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateChineseComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "PreferenceValue",
                schema: "Configuration",
                comment: "分层偏好值（Configuration）：按键、层级、角色和用户组合唯一；系统层无归属，角色层按角色保存，个人层按用户和角色保存",
                oldComment: "偏好在某一层上的值；Configuration 模块；系统层、角色层、用户在角色下的层，各层同一个键只有一行");

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
                oldComment: "改之前的值；此前用的是默认值时为空");

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
                oldComment: "改之后的值；恢复默认时为空");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "PreferenceValue",
                schema: "Configuration",
                comment: "偏好在某一层上的值；Configuration 模块；系统层、角色层、用户在角色下的层，各层同一个键只有一行",
                oldComment: "分层偏好值（Configuration）：按键、层级、角色和用户组合唯一；系统层无归属，角色层按角色保存，个人层按用户和角色保存");

            migrationBuilder.AlterColumn<string>(
                name: "OldValue",
                schema: "Configuration",
                table: "SystemSettingChange",
                type: "jsonb",
                nullable: true,
                comment: "改之前的值；此前用的是默认值时为空",
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
                comment: "改之后的值；恢复默认时为空",
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true,
                oldComment: "修改后保存的值");
        }
    }
}
